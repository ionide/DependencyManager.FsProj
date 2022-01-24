# DependencyManager.FsProj

This is still **experimental**.

Sample Dependency Manager for `dotnet fsi` for F# 5 preview 1. It extends `#r` syntax with `fsproj` dependency manager, so you can do `#r "fsproj: PATH_TO_FSPROJ.fsproj"` and it will load all references and files from the project.

```fsharp
#r "fsproj: ./test/test.fsproj"

let t = test.Say.hello "Chris"
printfn "RESULT: %s" t
```

## How to use sample

1. Have .NET 5 preview 1 installed
2. Alternatively, Start a Docker container with the latest preview interactively `docker run --rm -it -v "${pwd}:/app" -w "/app" mcr.microsoft.com/dotnet/core/sdk:5.0.100-preview bash` or run `run.ps1` with PowerShell.
3. Restore the dependencies `dotnet restore -s "${pwd}/lib`
4. Run `dotnet publish src/DependencyManager.FsProj/`
5. To run sample script from `test.fsx` file run `dotnet fsi --langversion:preview --compilertool:./src/DependencyManager.FsProj/bin/Debug/netstandard2.0/publish test.fsx`
6. Alternatively, start FSI in interactive mode with `dotnet fsi --langversion:preview --compilertool:./src/DependencyManager.FsProj/bin/Debug/netstandard2.0/publish` and type above script by hand.

## How to contribute

*Imposter syndrome disclaimer*: I want your help. No really, I do.

There might be a little voice inside that tells you you're not ready; that you need to do one more tutorial, or learn another framework, or write a few more blog posts before you can help me with this project.

I assure you, that's not the case.

This project has some clear Contribution Guidelines and expectations that you can [read here](https://github.com/Krzysztof-Cieslak/DependencyManager.FsProj/blob/master/CONTRIBUTING.md).

The contribution guidelines outline the process that you'll need to follow to get a patch merged. By making expectations and process explicit, I hope it will make it easier for you to contribute.

And you don't just have to write code. You can help out by writing documentation, tests, or even by giving feedback about this work. (And yes, that includes giving feedback about the contribution guidelines.)

Thank you for contributing!

## Contributing and copyright

The project is hosted on [GitHub](https://github.com/Krzysztof-Cieslak/DependencyManager.FsProj) where you can [report issues](https://github.com/Krzysztof-Cieslak/DependencyManager.FsProj/issues), fork
the project and submit pull requests.

The library is available under [MIT license](https://github.com/Krzysztof-Cieslak/DependencyManager.FsProj/blob/master/LICENSE.md), which allows modification and redistribution for both commercial and non-commercial purposes.

Please note that this project is released with a [Contributor Code of Conduct](CODE_OF_CONDUCT.md). By participating in this project you agree to abide by its terms.
