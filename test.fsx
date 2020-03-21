#r "fsproj: ./test/test.fsproj"

let t = test.Say.hello "Krzysztof"
printfn "RESULT: %s" t