#r @"fsproj: ./SomeLibWithProjectRefsAndNuget/SomeLibWithProjectRefsAndNuget.fsproj"

open Newtonsoft.Json
open SomeLib
open SomeLibWithProjectRefsAndNuget

Say.hello "Word"

Person.create "Albert" 76
|> JsonConvert.SerializeObject
|> printfn "%s"
