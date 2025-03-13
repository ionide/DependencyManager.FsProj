open System
open System.IO
open System.Text.Json

open FSharp.SystemCommandLine

open DependencyManager.FsProj

type Dummy() =
    do ()

let showUsage () =
    let installDir = typeof<Dummy>.Assembly.Location |> System.IO.Path.GetDirectoryName

    let jsonFriendlyInstallDir = installDir.Replace("\\", "/")

    [
        "Usage: DependencyManager.FsProj.exe <list of projects>"
        ""
        "This will generate a load script and output it to standard output."
        ""
        "To use as DependencyManager with fsi: "
        $"dotnet fsi --compilertool:{installDir} <script-file.fsx>"
        ""
        "To use as DependencyManager with ionide add following to your .vscode/settings.json:"
        "\"FSharp.FSIExtraInteractiveParameters\": ["
        $"    \"--compilertool:{jsonFriendlyInstallDir}\""
        "],"
        "\"FSharp.FSIExtraSharedParameters\": ["
        $"    \"--compilertool:{jsonFriendlyInstallDir}\""
        "]"
    ]
    |> List.iter (printfn "%s")

module Enum =
    open FSharp.Reflection

    let getAllValues<'a> =
        FSharpType.GetUnionCases typeof<'a>
        |> Array.map (fun case ->
            try
                FSharpValue.MakeUnion(case, [||]) :?> 'a
            with :? Reflection.TargetParameterCountException ->
                failwith $"{typeof<'a>.Name} is not an Enum. Case '{case.Name}' has some parameters.")
        |> List.ofArray

    let parse<'a> (text: string) =
        getAllValues<'a>
        |> List.tryFind (fun x -> (string x).ToLowerInvariant() = text.ToLowerInvariant())
        |> Option.defaultWith (fun () -> failwith $"Invalid value for {typeof<'a>.Name}: '{text}'")

[<RequireQualifiedAccess>]
type Output =
    | LoadScript
    | Json
    | Text

let toLoadScript verbose (result: CollectedDependencies) =
    [
        if verbose then
            for notification in result.Notifications do
                $"""//{notification.Replace("\n", "\n//")}"""
        for reference in result.References do
            FsProjDependencyManager.toHashRLine reference
        for source in result.Sources do
            FsProjDependencyManager.toLoadSourceLine source
    ]
    |> String.concat Environment.NewLine

let toText verbose (result: FsProjDependenciesResult) =
    [
        for proj in result.FsProjDependencies do
            proj.ProjectPath

            for reference in proj.References do
                $"  Reference: {reference}"

            for source in proj.Sources do
                $"  Source: {source}"
        if verbose then
            for notification in result.Notifications do
                $"""Error: {notification.Replace("\n", "\n       ")}"""
    ]
    |> String.concat Environment.NewLine

let showResult (output, verbose) (result: FsProjDependenciesResult) =
    match output with
    | Output.LoadScript -> toLoadScript verbose (FsProjDependencyManager.collectDependencies result)
    | Output.Json -> JsonSerializer.Serialize result
    | Output.Text -> toText verbose result
    |> printfn "%s"

let handler (currDir: DirectoryInfo, output: string, outOfProcess: bool, verbose: bool, fsprojs: FileInfo[]) =
    if Array.isEmpty fsprojs then
        showUsage ()
    else
        let projects = List.ofArray fsprojs

        if outOfProcess then
            FsProjDependencyManager.resolveDependenciesOutOfProcess currDir projects
        else
            FsProjDependencyManager.resolveDependencies currDir projects
        |> showResult (Enum.parse output, verbose)

[<EntryPoint>]
let main argv =
    rootCommand argv {
        description
            "Generates load script for a fsproj file or if referenced as compilertool in fsi can be used as 'DependencyManager'."

        inputs (
            Input.Option<DirectoryInfo>(
                [ "--dir"; "-d" ],
                Directory.GetCurrentDirectory() |> DirectoryInfo,
                "The engine to use to generate load script"
            ),
            Input.Option<string>([ "--output"; "-o" ], string Output.LoadScript, "The format of the output."),
            Input.Option<bool>([ "--out-of-process"; "-p" ], false, "The format of the output."),
            Input.Option<bool>([ "--verbose"; "-v" ], false, "Show verbose output."),
            Input.Argument<FileInfo[]>("The fsproj file to generate load script for")
        )

        setHandler handler
    }
