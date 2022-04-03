$nupkg=".nupkg"
$toolPath=".depman-fsproj"
Remove-Item -Recurse -ErrorAction SilentlyContinue $toolPath
dotnet pack -o $nupkg
dotnet tool install --tool-path $toolPath --add-source $nupkg --ignore-failed-sources depman-fsproj
& $toolPath\depman-fsproj.exe