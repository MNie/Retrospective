namespace Topic.Application.Topics

open System
open System.Text.Json
open Microsoft.Extensions.Logging
open Topic.Application
open Topic.Application.Configuration
open Topic.Application.Result
open Notification.Contract

[<RequireQualifiedAccess>]
module Update =
    let update
        (di: DI)
        topic
        token =
        async {
            let! result = Repository.update token di.config topic
            
            match result with
            | Choice1Of2 (DTOResult.Update data) ->
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
                di.logger.Log(LogLevel.Warning, $"Expected to get an update result, but got something else: {res}")
                return result
            | Choice2Of2 ex ->
                di.logger.Log(LogLevel.Error, $"Error occurred while processing data in repository: {ex.Message}")
                return result
        }
