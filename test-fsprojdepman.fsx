#r "nuget: Ionide.ProjInfo"
#r "nuget: Ionide.ProjInfo.ProjectSystem"

#load "./src/DependencyManager.FsProj/Extensions.fs"
#load "./src/DependencyManager.FsProj/DependencyManager.FsProj.fs"

open DependencyManager.FsProj

let depMan = FsProjDependencyManager(Some __SOURCE_DIRECTORY__)

//let projPath = Path.GetFullPath "./test/test.fsproj"
let projPath = Path.GetFullPath "C:/Users/tomas/source/farmer-1/src/Farmer/Farmer.fsproj"

let result = depMan.ResolveDependencies(__SOURCE_DIRECTORY__, "stdin.fsx", "stdin.fsx", [ projPath ], "net6.0")

result.Success