[<EntryPoint>]
let main argv =
    let installDir = 
        typeof<DependencyManager.FsProj.FsProjDependencyManager>.Assembly.Location 
        |> System.IO.Path.GetDirectoryName
    let jsonFriendlyInstallDir = installDir.Replace("\\", "/")
    printfn "To use with fsi run: "
    printfn $"dotnet fsi --compilertool:{installDir} <script-file.fsx>"
    printfn ""
    printfn "To use with ionide add following to your .vscode/settings.json:"
    printfn "\"FSharp.fsiExtraParameters\": ["
    printfn $"    \"--compilertool:{jsonFriendlyInstallDir}\""
    printfn "]"
    0