[xml]$buildProps = Get-Content -Path ".\Directory.Build.props"
$version = $buildProps.Project.PropertyGroup.Version
$nupkg=".nupkg"
$toolPath=".depman-fsproj"
Remove-Item -Recurse -ErrorAction SilentlyContinue $nupkg
Remove-Item -Recurse -ErrorAction SilentlyContinue $toolPath
dotnet pack -o $nupkg
dotnet tool install --tool-path $toolPath --add-source $nupkg --ignore-failed-sources --version $version depman-fsproj
& $toolPath\depman-fsproj.exe