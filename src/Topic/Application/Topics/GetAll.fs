namespace Topic.Application.Topics

open System.Threading
open Topic.Application

[<RequireQualifiedAccess>]
module GetAll =
    let get (config) (token: CancellationToken) =
        Repository.view token config

