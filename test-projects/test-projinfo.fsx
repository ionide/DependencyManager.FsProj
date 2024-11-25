#r "nuget: Ionide.ProjInfo, 0.68"
#r "nuget: Ionide.ProjInfo.ProjectSystem, 0.68"

open System
open System.IO
open Ionide.ProjInfo
open Ionide.ProjInfo.Types
open Ionide.ProjInfo.ProjectSystem

fsi.AddPrintTransformer(fun (fi: FileInfo) -> fi.FullName)
fsi.AddPrintTransformer(fun (di: DirectoryInfo) -> di.FullName)
fsi.AddPrintTransformer(fun (v: SemanticVersioning.Version) -> v.ToString())
fsi.AddPrintTransformer(fun (sdk: SdkDiscovery.DotnetSdkInfo) -> $"{sdk.Version}: {sdk.Path}")
fsi.AddPrintTransformer(fun (dt: DateTime) -> dt.ToString("dd.MM.yyyy HH:mm"))
fsi.AddPrintTransformer(fun (projOpts: ProjectOptions) -> System.IO.Path.GetFileName(projOpts.ProjectFileName))

#load "../src/DependencyManager.FsProj/Extensions.fs"

open Extensions

// let dotnetExe, sdk = DirectoryInfo __SOURCE_DIRECTORY__ |> SdkSetup.getSdkFor
// printfn $"DotNet: {dotnetExe}"
// printfn $"SDK: {sdk.Version}"
// let loader : IWorkspaceLoader = SdkSetup.setupForSdk (dotnetExe, sdk) |> WorkspaceLoader.Create
// let projects = [ Path.GetFullPath("./SimpleLib/SimpleLib.fsproj") ]
// projects |> List.iter (DotNet.restore dotnetExe)
// loader.LoadProjects projects

let dotnetExe, sdk = SdkSetup.getSdkFor (DirectoryInfo __SOURCE_DIRECTORY__)
let toolsPath = SdkSetup.setupForSdk (dotnetExe, sdk)
let (loader: IWorkspaceLoader) = WorkspaceLoader.Create(toolsPath)

//let projPath = Path.GetFullPath "./test/test.fsproj"
let projPath = Path.GetFullPath "./test-projects/SimpleLib/SimpleLib.fsproj"

let projs = loader.LoadProjects [ projPath ] |> List.ofSeq |> List.rev

projs.[0].ProjectFileName
projs.[0].SourceFiles
