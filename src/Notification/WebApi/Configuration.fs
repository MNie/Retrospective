namespace WebApi.Configuration

type Name = string
type MessageType = string
type ApiKey = string

[<AllowNullLiteral>]
type AblyChannelConfig () =
    member val Name: Name = "" with get, set
    member val MessageType: MessageType = "" with get, set

[<AllowNullLiteral>]
type AblyChannels () =
    member val Notification: AblyChannelConfig = null with get, set
    member val Push: AblyChannelConfig = null with get, set
    
[<AllowNullLiteral>]
type AblyConfig () =
    member val Channels: AblyChannels = null with get, set
    member val ApiKey: ApiKey = null with get, set

type Config () =
    member val Ably: AblyConfig = null with get, set