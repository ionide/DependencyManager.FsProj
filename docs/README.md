# DependencyManager.FsProj

This nuget package enables loading `.fsproj` files in `.fsx` scripts.
It extends `#r` syntax with `fsproj` dependency manager, so you can do `#r "fsproj: PATH_TO_FSPROJ.fsproj"` and it will load all references and files from the project.

Sample:

```fsharp
#r "fsproj: ./test/test.fsproj"

let t = test.Say.hello "Chris"
printfn "RESULT: %s" t
```

## Installation

Run `dotnet tool install --global depman-fsproj` and once it is installed run `depman-fsproj` for instructions.
