$workdir=".workdir"
$outdir=".depman"
Remove-Item -Recurse -ErrorAction SilentlyContinue $workdir
dotnet new classlib -o $workdir -n depman
dotnet add $workdir reference ./src/DependencyManager.FsProj.fsproj
dotnet publish $workdir -o $outdir
Remove-Item -Recurse $workdir