module WebApi.Notifications.Save

open System.Text.Json
open System.Text.Json
open System.Text.Json.Serialization
open IO.Ably
open Microsoft.Extensions.Logging
open Newtonsoft.Json.Linq
open WebApi.Configuration
open Notification.Contract

let private serializerOpt =
    let options = JsonSerializerOptions()
    JsonFSharpConverter(
        unionEncoding = (
            // Base encoding:
            JsonUnionEncoding.Untagged  
            // Additional options:
            ||| JsonUnionEncoding.UnwrapOption
            ||| JsonUnionEncoding.UnwrapRecordCases
        )
    )
    |> options.Converters.Add
    
    options

let internal handle (ably: AblyRealtime) (ablyConfig: AblyConfig) (msg: obj) (logger: ILogger) =
    try
        let deserialized = (msg :?> JObject).ToObject<Notification.Contract.Message>()
        async {
            let channel = ably.Channels.Get ablyConfig.Channels.Push.Name
            let msg = JsonSerializer.Serialize(deserialized, serializerOpt)
            let! _ = channel.PublishAsync (ablyConfig.Channels.Push.MessageType, msg) |> Async.AwaitTask
            ()
        }
    with er ->
        logger.LogError $"Error occured while processing notification message: {er.Message}"
        async {
            let channel = ably.Channels.Get ablyConfig.Channels.Push.Name
            let! _ = channel.PublishAsync (ablyConfig.Channels.Push.MessageType, er.Message) |> Async.AwaitTask
            ()
        }