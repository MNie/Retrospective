namespace WebApi.Controllers

open System
open System.Threading
open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open MongoDB.Bson
open Topic.Application.Result
open Topic.Application.Configuration
open Topic.Contract

[<ApiController>]
[<Route("[controller]")>]
type TopicController (di: DI, logger: ILogger<TopicController>) =
    inherit ControllerBase()

    let ok r = OkObjectResult r :> IActionResult
    let badRequest = StatusCodeResult 400 :> IActionResult
    member private __.badRequestWithData data  = __.StatusCode (400, data) :> IActionResult

    member private __.mapToResponse resolveIt result =
        match result with
        | Choice1Of2 c ->
            resolveIt c
        | Choice2Of2 (exn: exn) ->  __.StatusCode (500, (sprintf "msg: %s, stacktrace: %s" exn.Message exn.StackTrace)) :> IActionResult

    [<HttpPost>]
    member __.Post (topic: Topic.Contract.Topic) =
        async {
            let! result = Topic.Application.Topics.Save.save di topic CancellationToken.None
            return __.mapToResponse (fun x -> match x with | DTOResult.Create v -> ok v | _ -> badRequest) result
        } |> Async.StartAsTask
        
    [<HttpPut>]
    member __.Put (topic: Topic.Contract.Topic) =
        async {
            let! result = Topic.Application.Topics.Update.update di topic CancellationToken.None
            return __.mapToResponse (fun x -> match x with | DTOResult.Create v -> ok v | _ -> badRequest) result
        } |> Async.StartAsTask
        
    [<HttpGet>]
    member __.Sample() =
        async {
            let topic: Topic.Contract.Topic = {
                Name = Name "test"
                Id = Id (ObjectId.GenerateNewId().ToString())
                Creator = Creator "creator"
                Description = Description "todo"
                Done = Done false
            }
            let! result = Topic.Application.Topics.Save.save di topic CancellationToken.None
            return __.mapToResponse (fun x -> match x with | DTOResult.Create v -> ok v | _ -> badRequest) result
        } |> Async.StartAsTask
