#r "fsproj: ./test/test.fsproj"

let t = test.Say.hello "Chris"
printfn "RESULT: %s" t