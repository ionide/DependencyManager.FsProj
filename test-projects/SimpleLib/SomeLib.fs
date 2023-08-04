namespace SomeLib

module Person =
    let create (name: string) (age: int) =
        {| Name = name; Age = age  |}
