#I "../src/DependencyManager.FsProj/bin/Debug/net9.0"
#r "DependencyManager.FsProj.dll"

open System.IO

open DependencyManager.FsProj

let (</>) p1 p2 = Path.Combine(p1, p2)

let projPath =
    __SOURCE_DIRECTORY__ </> "SimpleLib" </> "SimpleLib.fsproj" |> FileInfo

let baseDir = DirectoryInfo(__SOURCE_DIRECTORY__)
let result = FsProjDependencyManager.resolveDependencies baseDir [ projPath ]
// #r "nuget: Ionide.ProjInfo, 0.55.4"
// #r "nuget: Ionide.ProjInfo.ProjectSystem, 0.55.4"

// #load "../src/Extensions.fs"
// #load "../src/DependencyManager.FsProj.fs"

// open System
// open System.IO
// open DependencyManager.FsProj
// open Ionide.ProjInfo
// open Ionide.ProjInfo.Types

// fsi.AddPrintTransformer(fun (fi: FileInfo) -> fi.FullName)
// fsi.AddPrintTransformer(fun (di: DirectoryInfo) -> di.FullName)
// fsi.AddPrintTransformer(fun (v: SemanticVersioning.Version) -> v.ToString())
// fsi.AddPrintTransformer(fun (sdk: SdkDiscovery.DotnetSdkInfo) -> $"{sdk.Version}: {sdk.Path}")
// fsi.AddPrintTransformer(fun (dt: DateTime) -> dt.ToString("dd.MM.yyyy HH:mm"))
// fsi.AddPrintTransformer(fun (projOpts: ProjectOptions) -> System.IO.Path.GetFileName(projOpts.ProjectFileName))

// let depMan = FsProjDependencyManager(Some __SOURCE_DIRECTORY__)

// let projPath = Path.GetFullPath "./test/test.fsproj"

// let projs = FsProjDependencyManager.loadAllProjects __SOURCE_DIRECTORY__ [projPath]

// let sortedProjs = FsProjDependencyManager.sortByDependencies projs
// let sources = FsProjDependencyManager.getSourceFiles sortedProjs

// let result = depMan.ResolveDependencies(__SOURCE_DIRECTORY__, "stdin.fsx", "stdin.fsx", [ projPath ], "net6.0")

// let shared = projs |> List.find (fun p -> p.ProjectFileName.Contains("Shared"))

// projs |> List.filter (fun p -> p.ReferencedProjects |> List.exists (fun rp -> rp.ProjectFileName = shared.ProjectFileName))
