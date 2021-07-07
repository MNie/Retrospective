namespace Topic.Application.Configuration

open System.Text.Json
open System.Text.Json.Serialization
open IO.Ably
open Microsoft.Extensions.Logging

module internal Serialization =
    let internal serializerOpt =
        let options = JsonSerializerOptions()
        JsonFSharpConverter(
            unionEncoding = (
                // Base encoding:
                JsonUnionEncoding.Untagged  
                // Additional options:
                ||| JsonUnionEncoding.UnwrapOption
                ||| JsonUnionEncoding.UnwrapRecordCases
                ||| JsonUnionEncoding.UnwrapSingleCaseUnions
            )
        )
        |> options.Converters.Add
        
        options

type DbName = string
type CollectionConfig =
    {
        db: DbName
    }
        
open System
open BagnoDB

[<AllowNullLiteral>]
type MongoConfig () =
    member val Database: string = "" with get, set
    member val Host: string = "" with get, set
    member val Port: int = 0 with get, set
    member val User: string = "" with get, set
    member val Password: string = "" with get, set
    member val AuthDB: string = "" with get, set

module MongoConfig =
    let map (dto: MongoConfig) =
        let isEmpty s = String.IsNullOrWhiteSpace s
        match isEmpty dto.Password, isEmpty dto.User, isEmpty dto.AuthDB with
        | true, true, _
        | true, false, _
        | false, false, true
        | false, true, _->
            { host = dto.Host; port = dto.Port; user = None; password = None; authDb = None }
        | false, false, false ->
            { host = dto.Host; port = dto.Port; user = Some dto.User; password = Some dto.Password; authDb = Some dto.AuthDB }
            
type Name = string
type MessageType = string
type ApiKey = string

[<AllowNullLiteral>]
type AblyChannelConfig () =
    member val Name: Name = "" with get, set
    member val MessageType: MessageType = "" with get, set

[<AllowNullLiteral>]
type AblyChannels () =
    member val Topic: AblyChannelConfig = null with get, set
    member val Notification: AblyChannelConfig = null with get, set
    
[<AllowNullLiteral>]
type AblyConfig () =
    member val Channels: AblyChannels = null with get, set
    member val ApiKey: ApiKey = null with get, set

type DI = {
    config: (Config * CollectionConfig)
    ably: AblyRealtime
    ablyConfig: AblyChannels
    logger: ILogger
}

type Config () =
    member val MongoDb: MongoConfig = null with get, set
    member val Ably: AblyConfig = null with get, set