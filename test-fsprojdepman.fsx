#r "nuget: Ionide.ProjInfo"
#r "nuget: Ionide.ProjInfo.ProjectSystem"

#load "./src/DependencyManager.FsProj/Extensions.fs"
#load "./src/DependencyManager.FsProj/DependencyManager.FsProj.fs"

open DependencyManager.FsProj

let depMan = FsProjDependencyManager(Some __SOURCE_DIRECTORY__)

depMan.ResolveDependencies(__SOURCE_DIRECTORY__, "stdin.fsx", "stdin.fsx", [ "./test/test.fsproj"], "net6.0")