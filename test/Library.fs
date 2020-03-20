namespace test

open Newtonsoft.Json


module Say =
    let hello name =
        let o = {| X = 2; Y = "Hello" |}
        sprintf "%s" (JsonConvert.SerializeObject o)
