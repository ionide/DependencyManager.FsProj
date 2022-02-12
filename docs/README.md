# DependencyManager.FsProj

This nuget package enables loading `.fsproj` files in `.fsx` scripts.
It extends `#r` syntax with `fsproj` dependency manager, so you can do `#r "fsproj: PATH_TO_FSPROJ.fsproj"` and it will load all references and files from the project.

To use it download or install nuget package in some directory and then use "--compilertool:DIRECTORY_CONTAINING_DEPENDENCY_MANAGER"
or if you are using ionide in Visual Studio Code add following to your User or Workspace  `settings.json`.

```json
  "FSharp.fsiExtraParameters": [
    "--compilertool:DIRECTORY_CONTAINING_DEPENDENCY_MANAGER"
  ]
```

To download the nuget package run `echo '#r "nuget: DependencyManager.FsProj"' | dotnet fsi`.

Now run `dotnet nuget locals all --list` and look for `global-packages` to find out the location of the installed nuget package. On windows it will be something like `c:\Users\YOUR_USERNAME\.nuget\packages`.

Add following to your workspace or user settings.json.

```json
  "FSharp.fsiExtraParameters": [
    "--compilertool:c:\Users\YOUR_USERNAME\.nuget\packages\dependencymanager.fsproj\0.1.1\lib\net5.0\"
  ]
```
