namespace DependencyManager.FsProj

open System
open System.Diagnostics
open System.Text.Json
open System.IO
open Extensions
open Ionide.ProjInfo

type FsProjDependencies =
    {
        ProjectPath: string
        References: string list
        Sources: string list
    }

type FsProjDependenciesResult = 
    {
        FsProjDependencies: FsProjDependencies list
        Notifications: string []
    }

type CollectedDependencies =
    {
        Projects: string list
        References: string list
        Sources: string list
        Notifications: string []
    }

module FsProjDependencyManager =
    open System.Text.RegularExpressions

    let getOrCreateWorkingDirectory (outputDirectory: string option) =
        // Calculate the working directory for dependency management
        //   if a path wasn't supplied to the dependency manager then use the temporary directory as the root
        //   if a path was supplied if it was rooted then use the rooted path as the root
        //   if the path wasn't supplied or not rooted use the temp directory as the root.
        let directory =
            let path =
                Path.Combine(Process.GetCurrentProcess().Id.ToString() + "--" + Guid.NewGuid().ToString())

            match outputDirectory with
            | None -> Path.Combine(Path.GetTempPath(), path)
            | Some v ->
                if Path.IsPathRooted(v) then
                    Path.Combine(v, path)
                else
                    Path.Combine(Path.GetTempPath(), path)

        lazy
            try
                if not (Directory.Exists(directory)) then
                    Directory.CreateDirectory(directory) |> ignore

                directory
            with _ ->
                directory

    let getProjectPaths (scriptDir: DirectoryInfo) (packageManagerTextLines: string list) =
        packageManagerTextLines
        |> List.map (fun line ->
            if Path.IsPathRooted line then
                line
            else
                Path.Combine(scriptDir.FullName, line) |> Path.GetFullPath)

    let loadProjects (baseDir: DirectoryInfo) (projects: FileInfo list) =
        if List.isEmpty projects then
            { FsProjDependencies = []; Notifications = [| "No projects to load" |] }
        else
            let notifications = ResizeArray()
            let dotnetExe, sdk = baseDir |> SdkSetup.getSdkFor
            notifications.Add $"Using SDK: {sdk.Version} from {sdk.Path}"
            let toolsPath = Init.init baseDir (Some dotnetExe)
            let existingProjects, notFoundProjects = projects |> List.partition (fun fi -> fi.Exists)
            for notFoundProject in notFoundProjects do
                notifications.Add $"Project not found: {notFoundProject.FullName}"

            for proj in existingProjects do
                let result = DotNet.restore dotnetExe proj
                if result.ExitCode <> 0 then
                    notifications.Add $"Failed to restore project: {proj.FullName}"
                    notifications.Add result.StdOut
                    notifications.Add result.StdErr
                else
                    notifications.Add $"Restored project: {proj.FullName}"

            let loader: IWorkspaceLoader = WorkspaceLoader.Create toolsPath
            use _ = loader.Notifications.Subscribe(fun n -> notifications.Add $"%A{n}")
            let result = 
                existingProjects
                |> List.map (fun fi -> fi.FullName)
                |> loader.LoadProjects
                |> List.ofSeq
                |> List.map (fun proj ->
                    notifications.Add $"Loaded project: {proj.ProjectId}: {proj.ProjectFileName}"
                    notifications.Add  $"  ItemCount: {proj.Items.Length}"
                    {
                        ProjectPath = proj.ProjectFileName
                        Sources = proj.SourceFiles
                        References = proj.PackageReferences |> List.map (fun pref -> pref.FullPath)
                    })
            { FsProjDependencies = result; Notifications = notifications.ToArray() }

    let filterOut (suffixes: string list) (paths: string list) =
        paths
        |> List.filter (fun path -> suffixes |> List.exists (fun suffix -> not (path.EndsWith(suffix))) |> not)

    let reAssemblyAttributes = Regex @"(\.NETCoreApp|\.NETStandard),Version=v\d+\.\d+\.AssemblyAttributes.fs$"
    let reAssemblyInfo projectName = Regex @$"{projectName}.AssemblyInfo.fs$"

    let filterOutGeneratedSourceFiles (projectPath: string) (sources: string list) =
        let projectName = Path.GetFileNameWithoutExtension projectPath
        let projectDir = Path.GetDirectoryName projectPath
        let objDir = Path.Combine (projectDir, "obj")

        let isGeneratedFile projectName (objDir: string) (path: string) =
            let fileName = Path.GetFileName path
            let isInObjDir = path.StartsWith objDir
            let matchesGeneratedFileName =
                [ reAssemblyAttributes; reAssemblyInfo projectName ]
                |> List.exists (fun re -> re.IsMatch fileName)
            isInObjDir && matchesGeneratedFileName

        sources
        |> List.filter (isGeneratedFile projectName objDir >> not)

    let filterOutFrameworkReferences (references: string list) =
        let isFSharpCore (path: string) =
            Path.GetFileName path = "FSharp.Core.dll"

        references
        |> List.filter (isFSharpCore >> not)

    let collectDependencies (result: FsProjDependenciesResult) =
        let projsDependencies = result.FsProjDependencies
        {
            Projects = projsDependencies |> List.map (fun p -> p.ProjectPath)
            References = projsDependencies |> List.collect (fun p -> filterOutFrameworkReferences p.References) |> List.distinct
            Sources = projsDependencies |> List.collect (fun p -> filterOutGeneratedSourceFiles p.ProjectPath p.Sources) |> List.distinct
            Notifications = result.Notifications
        }

    let toHashRLine package = "#r @\"" + package + "\""
    let toLoadSourceLine sourceFile = "#load @\"" + sourceFile + "\""

    let resolveDependencies (baseDir: DirectoryInfo) (projects: FileInfo list) =
        loadProjects baseDir projects

    type DummyForGettingAssmbly() = do ()
    let getDependencyManagerPath () = typeof<DummyForGettingAssmbly>.Assembly.Location

    let resolveDependenciesOutOfProcess (baseDir: DirectoryInfo) (projects: FileInfo list) =
        let dotnet, _sdk = SdkSetup.getSdkFor baseDir
        let depmanDll = getDependencyManagerPath()
        let projectsAsArgs = projects |> List.map (fun fi -> fi.FullName) |> String.concat " "
        let result = Process.execute baseDir dotnet $"{depmanDll} -o json {projectsAsArgs}"
        JsonSerializer.Deserialize<FsProjDependenciesResult>(result.StdOut)

    let makeScriptFromReferences references =
        [
            "// Generated from #r \"fsproj:Package References\""
            "// ============================================"
            "// References"
            "//"
            yield! references |> List.map (fun r -> $"#r @\"{r}\"")
        ]
        |> String.concat Environment.NewLine

    let emitFile filePath (body: string) =
        try
            File.WriteAllText(filePath, body)
        with _ ->
            ()

/// A marker attribute to tell FCS that this assembly contains a Dependency Manager, or
/// that a class with the attribute is a DependencyManager
[<AttributeUsage(AttributeTargets.Assembly ||| AttributeTargets.Class, AllowMultiple = false)>]
type DependencyManagerAttribute() =
    inherit Attribute()

module Attributes =
    [<assembly: DependencyManagerAttribute>]
    do ()

type ScriptExtension = string
type HashRLines = string seq
type TFM = string

/// The results of ResolveDependencies
type ResolveDependenciesResult
    (
        success: bool,
        stdOut: string array,
        stdError: string array,
        resolutions: string seq,
        sourceFiles: string seq,
        roots: string seq
    ) =

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

/// the type _must_ take an optional output directory
[<DependencyManager>]
type DependencyManagerFsProj(outputDirectory: string option) =
    let workingDirectory =
        FsProjDependencyManager.getOrCreateWorkingDirectory outputDirectory

    member val Key = "fsproj" with get
    member val Name = "FsProj Dependency Manager (Using Ionide.ProjInfo)" with get

    member _.HelpMessages =
        [|
            """    #r "fsproj: ./src/MyProj/MyProj.fsproj"; // loads all sources and packages from project MyProj.fsproj"""
        |]

    member _.ResolveDependencies
        (
            scriptDir: string,
            mainScriptName: string,
            scriptName: string,
            packageManagerTextLines: HashRLines,
            targetFramework: TFM
        ) : ResolveDependenciesResult =

        let baseDir = DirectoryInfo scriptDir

        try
            let projects, missingProjects =
                List.ofSeq packageManagerTextLines
                |> FsProjDependencyManager.getProjectPaths baseDir
                |> List.map FileInfo
                |> List.partition (fun fi -> fi.Exists)

            if not (List.isEmpty missingProjects) then
                let missingProjsStr = missingProjects |> List.map (fun fi -> fi.FullName) |> String.concat Environment.NewLine
                failwith $"Missing project file(s): {missingProjsStr}"

            let result =
                FsProjDependencyManager.resolveDependenciesOutOfProcess baseDir projects
                |> FsProjDependencyManager.collectDependencies

            let references = result.References
            let sources = result.Sources

            let generatedScriptName = $"{Guid.NewGuid()}.fsx"
            let generatedScriptPath = Path.Combine (workingDirectory.Value, generatedScriptName)
            let generatedScriptBody = FsProjDependencyManager.makeScriptFromReferences references
            FsProjDependencyManager.emitFile generatedScriptPath generatedScriptBody

            ResolveDependenciesResult(true, [| |], [| |], references, generatedScriptPath :: sources, [])
        with e ->
            printfn "exception while resolving dependencies: %s" (string e)
            ResolveDependenciesResult(false, [||], [| e.ToString() |], [], [], [])
