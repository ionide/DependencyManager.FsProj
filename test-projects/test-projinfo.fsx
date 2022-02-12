#r "nuget: Ionide.ProjInfo"
#r "nuget: Ionide.ProjInfo.ProjectSystem"

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

#load "../src/Extensions.fs"

open Extensions

let dotnetExe, sdk = SdkSetup.getSdkFor (DirectoryInfo __SOURCE_DIRECTORY__)
let toolsPath = SdkSetup.setupForSdk (dotnetExe, sdk)

System.Diagnostics.Process.Start(dotnetExe.FullName, [ "restore"; "./test/test.fsproj" ])

let (loader: IWorkspaceLoader) = WorkspaceLoader.Create(toolsPath)

//let projPath = Path.GetFullPath "./test/test.fsproj"
let projPath = Path.GetFullPath "C:/Users/tomas/source/farmer-1/src/Farmer/Farmer.fsproj"
loader.LoadProjects [projPath]
let proj = loader.LoadProjects [ projPath ] |> Seq.head

proj.TargetFramework
proj.TargetPath
proj.ProjectSdkInfo
proj.Items
proj.SourceFiles
proj.ReferencedProjects
proj.PackageReferences