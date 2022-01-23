#r @"fsproj: C:\Users\tomas\source\farmer-1\src\Farmer\Farmer.fsproj"

open Farmer
open Farmer.Builders

let storage = storageAccount {
    name "hello"
}

let x = arm {
    add_resource storage
}

x.Template |> Writer.toJson