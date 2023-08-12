module Tests

open System
open System.IO
open Expecto
open Expecto.Flip
open DependencyManager.FsProj

let (</>) p1 p2 = Path.Combine(p1, p2)
let rootDir = Path.GetFullPath (__SOURCE_DIRECTORY__ </> "..")
let testProjectsDir = rootDir </> "test-projects"
let simpleLibDir = testProjectsDir </> "SimpleLib"
let simpleLibPath = simpleLibDir </> "SimpleLib.fsproj"
let someLibWithNugetPackagesDir = testProjectsDir </> "SomeLibWithNugetPackages"
let someLibWithNugetPackagesPath = someLibWithNugetPackagesDir </> "SomeLibWithNugetPackages.fsproj"
let someLibWithProjectRefsAndNugetDir = testProjectsDir </> "SomeLibWithProjectRefsAndNuget"
let someLibWithProjectRefsAndNugetPath = someLibWithProjectRefsAndNugetDir </> "SomeLibWithProjectRefsAndNuget.fsproj"

let withNotifications (notifications: string[]) msg =
  [
    yield msg
    yield! notifications
  ]
  |> String.concat Environment.NewLine

[<Tests>]
let tests =
  testList "Load test-projects" [
    
    testCase "Load test-projects/SimpleLib" <| fun _ ->
      let result = FsProjDependencyManager.resolveDependenciesOutOfProcess (DirectoryInfo rootDir) [ FileInfo simpleLibPath ]
      let withNotifications = withNotifications result.Notifications
      Expect.hasLength result.FsProjDependencies 1 (withNotifications "Result contains only one project")
      let projDependencies = result.FsProjDependencies |> List.head
      projDependencies.ProjectPath |> Expect.equal (withNotifications "Should be SimpleLib.fsproj") simpleLibPath
      projDependencies.Sources |> Expect.contains (withNotifications "Should contain SomeLib.fs") (simpleLibDir </> "SomeLib.fs")
    
    testCase "Load test-projects/SomeLibWithNugetPackages" <| fun _ ->
      let result = FsProjDependencyManager.resolveDependenciesOutOfProcess (DirectoryInfo rootDir) [ FileInfo someLibWithNugetPackagesPath ]
      let withNotifications = withNotifications result.Notifications
      Expect.hasLength result.FsProjDependencies 1 (withNotifications "Result contains only one project")
      let projDependencies = result.FsProjDependencies |> List.head
      projDependencies.ProjectPath |> Expect.equal (withNotifications "Should be SomeLibWithNugetPackages.fsproj")  someLibWithNugetPackagesPath
      projDependencies.Sources |> Expect.contains (withNotifications "Should contain SomeLibWithNugetPackages.fs")  (someLibWithNugetPackagesDir </> "SomeLibWithNugetPackages.fs")
      projDependencies.References |> Expect.exists (withNotifications "Should contain NewtonsoftJson") (fun r -> Path.GetFileNameWithoutExtension r = "Newtonsoft.Json")

    testCase "Load test-projects/SomeLibWithProjectRefsAndNuget" <| fun _ ->
      let result = FsProjDependencyManager.resolveDependenciesOutOfProcess (DirectoryInfo rootDir) [ FileInfo someLibWithProjectRefsAndNugetPath ]
      let notifications = result.Notifications |> String.concat Environment.NewLine
      let withNotifications = withNotifications result.Notifications
      Expect.hasLength result.FsProjDependencies 3 (withNotifications "Result contains all 3 projects")
      result.FsProjDependencies |> Expect.exists (withNotifications "Should contain SomeLibWithProjectRefsAndNugetPath.fsproj") (fun projDependencies -> projDependencies.ProjectPath = someLibWithProjectRefsAndNugetPath)
      result.FsProjDependencies |> Expect.exists (withNotifications "Should contain SomeLibWithNugetPackages.fsproj") (fun projDependencies -> projDependencies.ProjectPath = someLibWithNugetPackagesPath)
      result.FsProjDependencies |> Expect.exists (withNotifications "Should contain SimpleLib.fsproj") (fun projDependencies -> projDependencies.ProjectPath = simpleLibPath)
  ]
