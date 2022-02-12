# DependencyManager.FsProj

This nuget package enables loading `.fsproj` files in `.fsx` scripts.
It extends `#r` syntax with `fsproj` dependency manager, so you can do `#r "fsproj: PATH_TO_FSPROJ.fsproj"` and it will load all references and files from the project.

## Installation

If you are using powershell you can use following script to install dependency manager to current directory. Probably a similar script for linux shell will also work.

```powershell
$workdir=".workdir"
$outdir=".depman"
dotnet new classlib -o $workdir -n depman
dotnet add $workdir package DependencyManager.FsProj
dotnet publish $workdir -o $outdir
Remove-Item -Recurse $workdir
```

After that you can call `dotnet fsi --compilertool:./.depman script.fsx` or add following line to vscode settings.json.

```json
  "FSharp.fsiExtraParameters": [
    "--compilertool:./.depman"
  ]
```
