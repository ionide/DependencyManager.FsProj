namespace DependencyManager.FsProj

open ProjectSystem
open FSharp.Compiler.SourceCodeServices

module Internals =
    open System

    /// A marker attribute to tell FCS that this assembly contains a Dependency Manager, or
    /// that a class with the attribute is a DependencyManager
    [<AttributeUsage(AttributeTargets.Assembly ||| AttributeTargets.Class , AllowMultiple = false)>]
    type DependencyManagerAttribute() =
        inherit Attribute()

    [<assembly: DependencyManagerAttribute()>]
    do ()

    type ScriptExtension = string
    type HashRLines = string seq
    type TFM = string

    [<DependencyManager>]
    /// the type _must_ take an optional output directory
    type FsProjDependencyManager(outputDir: string option) =

        let checker = FSharpChecker.Create()

        let projectController = ProjectController(checker)

        member val Name = "FsProj Dependency Manager" with get
        member val Key = "fsproj" with get

        member _.ResolveDependencies(scriptExt : ScriptExtension, packageManagerTextLines : HashRLines, tfm: TFM) : (bool * string list * string list * string list) =
            let fsproj = Seq.head packageManagerTextLines

            let res =
                projectController.LoadProject fsproj ignore FSIRefs.TFM.NetCore (fun _ _ _ -> ())
                |> Async.RunSynchronously

            let refs, files =
                match res with
                | ProjectResponse.Project proj ->
                    proj.references, proj.projectFiles
                | ProjectResponse.ProjectError(errorDetails) ->
                    printfn "ERROR: %A" errorDetails
                    [], []
                | ProjectResponse.ProjectLoading(projectFileName) ->
                    [], []
                | ProjectResponse.WorkspaceLoad(finished) ->
                    [], []

            let refsToWrite =
                refs
                |> List.map (sprintf "#r @\"%s\"")

            let filePath = IO.Path.ChangeExtension(IO.Path.GetTempFileName (), "fsx")

            IO.File.WriteAllLines(filePath, refsToWrite)

            (true, List.empty<string>, [filePath; yield! files], List.empty<string> )