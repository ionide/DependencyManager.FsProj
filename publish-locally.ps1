$workdir=".workdir"
$outdir=".depman"
Remove-Item -Recurse -ErrorAction SilentlyContinue $workdir
dotnet new classlib -o $workdir -n depman
dotnet add $workdir package DependencyManager.FsProj
dotnet publish $workdir -o $outdir
Remove-Item -Recurse $workdir