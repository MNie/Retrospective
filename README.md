# Retrospective

## Architecture

The solution contains 5 parts:
- `Fascade` - the first layer of an application from a client POV receives all requests from the front-end (TS + Angular) and passes them further,
- `Notifications` - Standalone service that collects all the data that he received via the `Notifications` channel, parse it, and pass it as a `push message`,
- `Topic` - service which is responsible for collecting data from `Fascade`, save it to MongoDB and pass notification to a `Notification` service,
- `Storage` - MongoDB instance which runs in a docker container,
- `Front-end` - front-end part of an application that is located in the `Fascade` project.

## Flow

A user could create a topic for a retrospective. When the `submit` button would be hit, the request should go to `Fascade`. Fascade maps it from DTO to a `Topic.Contract` class
and passes it to `Topic` service via `Topic.save` Ably channel.

`Topic` service listens to all `Topic.save` or `Topic.update` messages and handles them. If handling result in success it passes a notification message to a `Notification`
service via the `Notification.notify` channel.

`Notification` receives all messages via the `Notifications.notify` channel, handles them, and passes them to all "clients" via the `Notifications.push` channel.
`Front-end` application listens to `Notifications.push` messages on the `Home` screen and pushes received data to the table visible for a user. 

In the `activity` tab user could see all messages that comms via `Notifications` channel as a "feed". History for `Notifications` channel should be enabled on Ably.io

## Communication

Is done via 2 channels with 2 message types:

- Channel: Topic, MessageType: save. `Fascade` -> `Topic`,
- Channel: Topic, MessageType: update. `Fascade` -> `Topic`,
- Channel: Notifications, MessageType: notify. `Topic` -> `Notification`,
- Channel: Notifications, MessageType: push. `Notifications` -> `Fascade (Front-end)`,


## How to run

- `docker compose up`,
- or run separate services in JetBrains Rider/VS Studio/VS Code or via `docker run`,
- fill `Ably.ApiKey` in appsettings.json files with your Ably.io ApiKey (mind that Ably is used in every microservice, so you need to update 3x AppSettings files).

## Additional things

- Health checks UI should be available at: `http://localhost:{serviceport}/ui-health`,
- Health checks UI API should be available at: `http://localhost:{serviceport}/ui-API-health`,
- Health checks should be available at: `http://localhost:{serviceport}/health`,
- Healthcheck for Ably is located in the `Utils` folder and it checks if we could connect to a channel (it is added to `Notification` and `Topic` services).

## Missing things

- ?
