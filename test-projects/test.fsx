#r @"fsproj: ./SimpleLib/SimpleLib.fsproj"
#r "nuget: Newtonsoft.Json"

open Newtonsoft.Json
open test

Person.create "Albert" 76
|> JsonConvert.SerializeObject
|> printfn "%s"
