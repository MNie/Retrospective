open Expecto

let tests = testList "WebApi - sample tests" [
    WebApiTests.tests
]

[<EntryPoint>]
let main argv =
    runTestsWithCLIArgs [] argv tests