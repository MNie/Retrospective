namespace WebApi.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging
open Notification.Contract

[<ApiController>]
[<Route("[controller]")>]
type NotificationController (logger : ILogger<NotificationController>) =
    inherit ControllerBase()

