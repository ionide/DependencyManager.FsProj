open System
open System.IO
open System.Text.Json

open DependencyManager.FsProj

let showUsage () =
    let installDir =
        typeof<DependencyManager.FsProj.DependencyManager>.Assembly.Location
        |> System.IO.Path.GetDirectoryName
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
        "\"FSharp.fsiExtraParameters\": ["
        $"    \"--compilertool:{jsonFriendlyInstallDir}\""
        "]"
    ] |> List.iter (printfn "%s")

let toLoadScript (references, sources) =
    List.concat [
        references |> List.map FsProjDependencyManager.toHashRLine
        sources |> List.map FsProjDependencyManager.toLoadSourceLine
    ]
    |> String.concat Environment.NewLine

let toJson (references, sources) =
    JsonSerializer.Serialize
        {
            References = references
            Sources = sources
        }

let getPackagesAndSources (projectPaths: string []) =
    let currentDir = Directory.GetCurrentDirectory()
    let projectPaths = List.ofArray projectPaths |> List.map (fun path -> Path.Combine(currentDir, path) |> Path.GetFullPath)
    let projects, _notifications = FsProjDependencyManager.loadProjects currentDir projectPaths
    let packages = FsProjDependencyManager.getPackageReferences projects
    let sources = FsProjDependencyManager.getSourceFiles projects

    packages, sources


[<EntryPoint>]
let main argv =
    if Array.isEmpty argv
    then showUsage ()
    else
        let options, projects = argv |> Array.partition (fun str -> str.StartsWith "--")
        let references, sources = getPackagesAndSources projects
        if options |> Array.contains "--json" then
            toJson (references, sources)
        else
            toLoadScript (references, sources)
        |> printfn "%s"
    0