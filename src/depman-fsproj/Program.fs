[<EntryPoint>]
let main argv =
    let installDir = 
        typeof<DependencyManager.FsProj.FsProjDependencyManager>.Assembly.Location 
        |> System.IO.Path.GetDirectoryName
    let jsonFriendlyInstallDir = installDir.Replace("\\", "/")
    printfn "To install add following to your .vscode/settings.json:"
    [
        "\"FSharp.fsiExtraParameters\": ["
        $"    \"--compilertool:{jsonFriendlyInstallDir}\""
        "]"
    ] 
    |> String.concat "\n"
    |> printfn "%s"
    0