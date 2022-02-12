namespace DependencyManager.FsProj

open System
open System.Diagnostics
open System.IO
open Ionide.ProjInfo
open Ionide.ProjInfo.Types
open Ionide.ProjInfo.ProjectSystem
open FSharp.DependencyManager.Nuget
open Extensions

/// A marker attribute to tell FCS that this assembly contains a Dependency Manager, or
/// that a class with the attribute is a DependencyManager
[<AttributeUsage(AttributeTargets.Assembly ||| AttributeTargets.Class , AllowMultiple = false)>]
type DependencyManagerAttribute() =
    inherit Attribute()

module Attributes =
    [<assembly: DependencyManagerAttribute()>]
    do ()

type ScriptExtension = string
type HashRLines = string seq
type TFM = string

/// The results of ResolveDependencies
type ResolveDependenciesResult (success: bool, stdOut: string array, stdError: string array, resolutions: string seq, sourceFiles: string seq, roots: string seq) =

    /// Succeded?
    member _.Success = success

    /// The resolution output log
    member _.StdOut = stdOut

    /// The resolution error log (* process stderror *)
    member _.StdError = stdError

    /// The resolution paths
    member _.Resolutions = resolutions

    /// The source code file paths
    member _.SourceFiles = sourceFiles

    /// The roots to package directories
    member _.Roots = roots

[<DependencyManager>]
/// the type _must_ take an optional output directory
type FsProjDependencyManager(outputDirectory: string option) =
    let workingDirectory =
        // Calculate the working directory for dependency management
        //   if a path wasn't supplied to the dependency manager then use the temporary directory as the root
        //   if a path was supplied if it was rooted then use the rooted path as the root
        //   if the path wasn't supplied or not rooted use the temp directory as the root.
        let directory =
            let path = Path.Combine(Process.GetCurrentProcess().Id.ToString() + "--"+ Guid.NewGuid().ToString())
            match outputDirectory with
            | None -> Path.Combine(Path.GetTempPath(), path)
            | Some v ->
                if Path.IsPathRooted(v) then Path.Combine(v, path)
                else Path.Combine(Path.GetTempPath(), path)

        lazy
            try
                if not (Directory.Exists(directory)) then
                    Directory.CreateDirectory(directory) |> ignore
                directory
            with | _ -> directory

    let emitFile path content =
        try
            File.WriteAllText(path, content)
        with _ -> ()

    let loadAllProjects scriptDir packageManagerTextLines =
        let dotnetExe, sdk = SdkSetup.getSdkFor (DirectoryInfo scriptDir)
        let toolsPath = SdkSetup.setupForSdk (dotnetExe, sdk)
        let (loader: IWorkspaceLoader) = WorkspaceLoader.Create(toolsPath)
        let projectPaths = 
            packageManagerTextLines 
            |> Seq.map (fun line -> Path.Combine (scriptDir, line))
            |> Seq.distinct
            |> List.ofSeq
        loader.LoadAllProjectsRecursively(dotnetExe, projectPaths)

    let getSourceFiles projects =
        projects 
        |> Seq.collect (fun proj -> proj.SourceFiles) 
        |> Seq.distinct
        |> List.ofSeq

    let generateLoadScriptContent projects =
        let toScriptRef path = "#r @\"" + path + "\"" + Environment.NewLine

        let packages =
            projects
            |> Seq.collect (fun proj -> proj.PackageReferences)
            |> Seq.map (fun pref -> pref.FullPath)
        
        packages
        |> Seq.distinct
        |> Seq.map toScriptRef
        |> String.concat Environment.NewLine

    let getErrors (projects: ProjectOptions seq) =
        let notRestored =
            projects
            |> Seq.filter (fun proj -> not proj.ProjectSdkInfo.RestoreSuccess)
            |> Seq.map (fun proj -> proj.ProjectFileName)
            
        if not (Seq.isEmpty notRestored) then
            notRestored
            |> Seq.append ["Please restore following projects with 'dotnet restore': "]
            |> Array.ofSeq
        else [||]


    let generateDebugOutput scriptDir mainScriptName scriptName packageManagerTextLines targetFramework (projects: ProjectOptions seq) sourceFiles loadScriptContent =
        [|
            $"================================"
            $"WorkingDirectory: {workingDirectory.Value}"
            $"ScriptDir: {scriptDir}"
            $"MainScriptName: {mainScriptName}"
            $"ScriptName: {scriptName}"
            $"PackageManagerTextLines:"
            for line in packageManagerTextLines do
                $"  >{line}"
            $"TargetFramework: {targetFramework}"
            $"================================"
            $"All projects:"
            for proj in projects do
                $" >{proj.ProjectFileName}: {proj.ProjectSdkInfo.TargetFramework} (IsRestored: {proj.ProjectSdkInfo.RestoreSuccess})"
                $" >References:"
                for pr in proj.ReferencedProjects do
                    $"   >{pr.ProjectFileName}"
            $"Source files:"
            for sourceFile in sourceFiles do
                $" >{sourceFile}"
            $"Load script content:"
            $"--------------------------------"
            $"{loadScriptContent}"
            $"--------------------------------"
        |]
    
    member val Key = "fsproj" with get
    member val Name = "FsProj Dependency Manager" with get
    member _.HelpMessages = [|
        """    #r "fsproj: ./src/MyProj/MyProj.fsproj"; // loads all sources and packages from project MyProj.fsproj"""
    |]

    member _.ResolveDependencies(scriptDir: string, mainScriptName: string, scriptName: string, packageManagerTextLines: HashRLines, targetFramework: TFM) : ResolveDependenciesResult =
        try
            let allProjects = loadAllProjects scriptDir packageManagerTextLines
            let sourceFiles = getSourceFiles allProjects
            let loadScriptContent = generateLoadScriptContent allProjects
            let loadScriptPath = Path.Combine(workingDirectory.Value, $"load-dependencies-{Path.GetFileName mainScriptName}")

            emitFile loadScriptPath loadScriptContent

            let stdError = getErrors allProjects
            let output = generateDebugOutput scriptDir mainScriptName scriptName packageManagerTextLines targetFramework allProjects sourceFiles loadScriptContent
            
            ResolveDependenciesResult(true, output, stdError, [], [ loadScriptPath; yield! sourceFiles ], [])
        with e -> 
            printfn "exception while resolving dependencies: %s" (string e)
            ResolveDependenciesResult(false, [||], [| e.ToString() |], [], [], [])