#r "nuget: Ionide.ProjInfo, 0.55.4"
#r "nuget: Ionide.ProjInfo.ProjectSystem, 0.55.4"

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

#load "../src/Extensions.fs"

open Extensions

let dotnetExe, sdk = SdkSetup.getSdkFor (DirectoryInfo __SOURCE_DIRECTORY__)
let toolsPath = SdkSetup.setupForSdk (dotnetExe, sdk)

let (loader: IWorkspaceLoader) = WorkspaceLoader.Create(toolsPath)

let projPath = Path.GetFullPath "./test/test.fsproj"

let projs = loader.LoadProjects [ projPath ] |> List.ofSeq |> List.rev

projs |> List.map (fun p -> p.ProjectFileName)