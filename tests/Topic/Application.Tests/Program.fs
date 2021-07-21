open Expecto

let tests = testList "Application - sample tests" [
    MapperTests.tests
]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv tests