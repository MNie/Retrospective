namespace WebApi.Controllers

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

[<ApiController>]
[<Route("[controller]")>]
type NotificationController (logger : ILogger<NotificationController>) =
    inherit ControllerBase()

