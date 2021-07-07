interface AblyChannel {
  name: string
  messageType: string
}

interface AblyConfig {
  apiKey: string
  push: AblyChannel
}
