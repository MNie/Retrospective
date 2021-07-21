module WebApiTests

open System
open System.Net
open System.Text.Json
open Expecto
open Microsoft.AspNetCore.Mvc.Testing
open Microsoft.Extensions.Configuration
open MongoDB.Bson
open Topic.Contract

type TopicWebApiFactory() =
    inherit WebApplicationFactory<WebApi.Startup>()
    
    override __.ConfigureWebHost (builder) =
        builder.ConfigureAppConfiguration(
            fun ctx config ->
                let cfg = ConfigurationBuilder().AddJsonFile("appsettings.json").Build()
                        
                config.AddConfiguration(cfg) |> ignore
        ) |> ignore
    

let client =
    let factory = new TopicWebApiFactory()
    factory.CreateClient()

let tests = testList "WebApi" [
    testCase "sample" <| fun _ ->
        let result =
            client.GetAsync ("/topic")
            |> Async.AwaitTask
            |> Async.RunSynchronously
        let content =
            result.Content.ReadAsStringAsync()
            |> Async.AwaitTask
            |> Async.RunSynchronously
        
        Expect.equal result.StatusCode HttpStatusCode.InternalServerError ""
        Expect.stringStarts content "msg: One or more errors occurred. (Unable to authenticate using sasl protocol mechanism SCRAM-SHA-1.)" ""
    
    ptestCase "sample2" <| fun _ ->
        let expectedTopic: Topic.Contract.Topic = {
            Name = Name "test"
            Id = Id (ObjectId.GenerateNewId().ToString())
            Creator = Creator "creator"
            Description = Description "todo"
            Done = Done false
        }
        let result =
            client.GetAsync ("/topic")
            |> Async.AwaitTask
            |> Async.RunSynchronously
        let deserializedContent =
            result.Content.ReadAsStringAsync()
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> JsonSerializer.Deserialize<Topic.Contract.Topic>
        
        Expect.equal result.StatusCode HttpStatusCode.OK ""
        Expect.equal deserializedContent expectedTopic ""
]