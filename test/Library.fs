namespace test

open Newtonsoft.Json


module Say =
    let hello name =
        let o = {| X = 2; Y = sprintf "Hello %s" name |}
//        sprintf "%A" o
        sprintf "%s" (JsonConvert.SerializeObject o)
