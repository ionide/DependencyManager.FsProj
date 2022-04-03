namespace test

module Person =
    let create (name: string) (age: int) =
        {| Name = name; Age = age  |}
