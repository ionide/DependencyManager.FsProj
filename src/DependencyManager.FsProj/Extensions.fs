module Extensions

open System
open System.IO
open Ionide.ProjInfo
open Ionide.ProjInfo.Types

type String with

    member path.EnsureTrailer =
        if
            path.EndsWith(Path.DirectorySeparatorChar)
            || path.EndsWith(Path.AltDirectorySeparatorChar)
        then
            path
        else
            path + string Path.DirectorySeparatorChar

module Process =
    open System.Text
    open System.Diagnostics

    type ProcessResult =
        { ExitCode: int
          StdOut: string
          StdErr: string }

    let execute (workDir: DirectoryInfo) (filePath: FileInfo) (args: string) =
        let psi =
            new ProcessStartInfo(
                filePath.FullName,
                args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workDir.FullName
            )

        let proc = Process.Start(psi)
        let output = StringBuilder()
        let error = StringBuilder()
        proc.OutputDataReceived.Add(fun args -> output.Append(args.Data) |> ignore)
        proc.ErrorDataReceived.Add(fun args -> error.Append(args.Data) |> ignore)
        proc.BeginErrorReadLine()
        proc.BeginOutputReadLine()
        proc.WaitForExit()

        { ExitCode = proc.ExitCode
          StdOut = output.ToString()
          StdErr = error.ToString() }

module DotNet =
    let restore (dotnetExe: FileInfo) (projPath: FileInfo) =
        let workingDir = projPath.Directory
        Process.execute workingDir dotnetExe $"restore \"{projPath}\""

module SdkSetup =
    open System.Reflection
    open System.Runtime.Loader

    let private resolveFromSdkRoot (sdkRoot: DirectoryInfo) : Func<AssemblyLoadContext, AssemblyName, Assembly> =
        Func<AssemblyLoadContext, AssemblyName, Assembly>(fun assemblyLoadContext assemblyName ->
            let paths =
                [ Path.Combine(sdkRoot.FullName, assemblyName.Name + ".dll")
                  Path.Combine(sdkRoot.FullName, "en", assemblyName.Name + ".dll") ]

            match paths |> List.tryFind File.Exists with
            | Some path -> assemblyLoadContext.LoadFromAssemblyPath path
            | None -> null)

    let mutable private resolveHandler: Func<AssemblyLoadContext, AssemblyName, Assembly> =
        null

    let setupResolveHandler sdkRoot =
        if resolveHandler <> null then
            AssemblyLoadContext.Default.remove_Resolving resolveHandler

        resolveHandler <- resolveFromSdkRoot sdkRoot
        AssemblyLoadContext.Default.add_Resolving resolveHandler

    let setupForSdk ((dotnetExe: FileInfo), (sdk: SdkDiscovery.DotnetSdkInfo)) =
        let sdkRoot = sdk.Path
        let msbuild = System.IO.Path.Combine(sdkRoot.FullName, "MSBuild.dll")
        Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH", msbuild)
        Environment.SetEnvironmentVariable("MSBuildExtensionsPath", sdkRoot.FullName.EnsureTrailer)
        Environment.SetEnvironmentVariable("MSBuildSDKsPath", Path.Combine(sdkRoot.FullName, "Sdks"))
        // .net 6 sdk includes workload stuff and this breaks for some reason
        Environment.SetEnvironmentVariable("MSBuildEnableWorkloadResolver", "false")

        match System.Environment.GetEnvironmentVariable "DOTNET_HOST_PATH" with
        | null
        | "" -> Environment.SetEnvironmentVariable("DOTNET_HOST_PATH", dotnetExe.FullName)
        | _alreadySet -> ()

        setupResolveHandler sdkRoot
        ToolsPath msbuild

    let getSdkFor (dir: DirectoryInfo) =
        match Paths.dotnetRoot.Value with
        | None -> failwith "Could not find dotnet exe"
        | Some exe ->
            match SdkDiscovery.versionAt dir exe with
            | Error err -> failwith $"Could not find .NET SDK version for directory {dir.FullName}"
            | Ok sdkVersionAtPath ->
                let sdks = SdkDiscovery.sdks exe

                let matchingSdks =
                    sdks |> Array.skipWhile (fun { Version = v } -> v < sdkVersionAtPath)

                match matchingSdks with
                | [||] -> failwith $"Could not find .NET SDK {sdkVersionAtPath}. Please install it."
                | found -> exe, Array.head found
