namespace Topic.Application.Topics

open System
open System.Text.Json
open Microsoft.Extensions.Logging
open Newtonsoft.Json.Linq
open Topic.Application
open Topic.Application.Configuration
open Topic.Application.Result
open Topic.Contract
open Notification.Contract

type InvalidTopicFormat = exn

[<RequireQualifiedAccess>]
module Save =
    let save
        (di: DI)
        topic
        token =
        async {
            let! result = Repository.create token di.config topic
            
            match result with
            | Choice1Of2 (DTOResult.Create data) ->
                let serializedData = JsonSerializer.Serialize(data, Serialization.serializerOpt)
                let (Topic.Contract.Id id) = data.Id
                let msg = {
                    Id = Identifier (Guid.NewGuid())
                    RelatedId = Id id
                    AdditionalData = AdditionalData serializedData
                }
                let channel = di.ably.Channels.Get di.ablyConfig.Notification.Name
                let! pubResult = channel.PublishAsync (di.ablyConfig.Notification.MessageType, msg) |> Async.AwaitTask
                if pubResult.IsFailure then
                    return Choice2Of2 (pubResult.Error.AsException ())
                else return result
            | Choice1Of2 res ->
                di.logger.Log(LogLevel.Warning, $"Expected to get a create result, but got something else: {res}")
                return result
            | Choice2Of2 ex ->
                di.logger.Log(LogLevel.Error, $"Error occurred while processing data in repository: {ex.Message}")
                return result
        }
    
    let trySave
        (di: DI)
        (topic: obj)
        token =
        async {
            try
                let deserializedTopic = (topic :?> JObject).ToObject<Topic>()
                return! save di deserializedTopic token
            with msg ->
                return
                    msg.Message
                    |> InvalidTopicFormat
                    |> Choice2Of2
        }
    
