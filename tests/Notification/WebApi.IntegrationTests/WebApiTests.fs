module WebApiTests

open System.Net
open System.Text.Json
open Expecto
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.Extensions.Configuration

type NotificationWebApiFactory() =
    inherit WebApplicationFactory<WebApi.Startup>()
    
    override __.ConfigureWebHost (builder) =
        builder.ConfigureAppConfiguration(
            fun ctx config ->
                let cfg = ConfigurationBuilder().AddJsonFile("appsettings.json").Build()
                        
                config.AddConfiguration(cfg) |> ignore
        ) |> ignore
    

let client =
    let factory = new NotificationWebApiFactory()
    factory.CreateClient()

let tests = testList "WebApi" [
    testCase "sample" <| fun _ ->
        let result =
            client.GetAsync ("/noendpointhere")
            |> Async.AwaitTask
            |> Async.RunSynchronously
        
        Expect.equal result.StatusCode HttpStatusCode.NotFound ""
]