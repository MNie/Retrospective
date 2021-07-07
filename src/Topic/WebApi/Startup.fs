namespace WebApi

open System
open System.Net
open System.Text.Json.Serialization
open System.Threading
open System.Threading.Tasks
open Ably.HealthCheck
open HealthChecks.UI.Client
open IO.Ably
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Diagnostics.HealthChecks
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Diagnostics.HealthChecks
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.OpenApi.Models
open Topic.Application.Configuration

module DI =
    let consolidatedConfig (config: Config) = MongoConfig.map config.MongoDb, { db = config.MongoDb.Database }
    let create (config: Config) (ably: AblyRealtime) (logger: ILogger) =
        { config = consolidatedConfig config; ably = ably; ablyConfig = config.Ably.Channels; logger = logger }

type Startup(configuration: IConfiguration) =
    let corsPolicy = "defaultCorsPolicy"

    member _.Configuration = configuration
    
    member private this.ConfigureHandlers (di: DI) (logger: ILogger<Startup>) =
        let channel = di.ably.Channels.Get di.ablyConfig.Topic.Name
        channel.Subscribe(
            di.ablyConfig.Topic.MessageType,
            fun msg ->
                async {
                    match! Topic.Application.Topics.Save.trySave di msg.Data CancellationToken.None with
                    | Choice1Of2 result -> logger.LogDebug $"Process message: {result}"
                    | Choice2Of2 err -> logger.LogError $"Error occured while processing message: {err}"
                    ()
                } |> Async.RunSynchronously)
    
    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        let config = Config ()
        this.Configuration.Bind config
        let ably = new AblyRealtime (config.Ably.ApiKey)
        let loggerFactory =
             LoggerFactory.Create(fun builder ->
                builder.AddConsole() |> ignore               
             );
        let logger = loggerFactory.CreateLogger<Startup>()
        
        let di = DI.create config ably logger
        services.AddSingleton<DI> (fun _ -> di) |> ignore
        
        this.ConfigureHandlers di logger
        
        services.AddSwaggerGen(fun c ->
                c.SwaggerDoc("v1", OpenApiInfo(Title = "Topic Api", Version = "v1"))
        ) |> ignore
        
        services.AddHealthChecks()
            .AddCheck(
                "Ably Channel",
                AblyChannelHealthCheck(
                    ably,
                    "Topic"
                )
            )
            .AddCheck(
                "Ably Timer",
                AblyTimerHealthCheck(
                    ably,
                    "Topic",
                    TimeSpan.FromSeconds 1.,
                    TimeSpan.FromSeconds 1.
                )
            )
            .AddMongoDb((MongoConfig.map config.MongoDb).GetConnectionString(), name = "MongoDB")
        |> ignore
        services
            .AddHealthChecksUI(fun s ->
                s
                    .SetEvaluationTimeInSeconds(60)
                    .AddHealthCheckEndpoint("Self", $"http://{Dns.GetHostName()}/health")
                |> ignore)
            .AddInMemoryStorage() |> ignore
        services.AddControllers()
            .AddJsonOptions(fun options ->
                    options.JsonSerializerOptions.Converters.Add(JsonFSharpConverter(unionTagCaseInsensitive = false, unionTagName = "case", unionFieldsName = "fields"))
                    options.JsonSerializerOptions.ReferenceHandler <- ReferenceHandler.Preserve
                    )
        |> ignore
        services.AddCors (fun opt -> opt.AddPolicy(corsPolicy, fun b -> b.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader() |> ignore) ) |> ignore

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then app.UseDeveloperExceptionPage() |> ignore
        app.UseRouting() |> ignore
        app.UseSwagger(fun opt -> ()) |> ignore
        app.UseSwaggerUI(fun opt ->
                opt.SwaggerEndpoint("/swagger/v1/swagger.json", "Topic Api V1")
                opt.RoutePrefix <- String.Empty
        ) |> ignore
        app.UseCors (corsPolicy) |> ignore
        app.UseEndpoints(fun endpoints ->
                endpoints.MapControllers() |> ignore
                endpoints.MapHealthChecksUI(fun setup ->
                    setup.UIPath <- "/ui-health"
                    setup.ApiPath <- "/api-ui-health"
                ) |> ignore
                endpoints.MapHealthChecks(
                    "/health",
                    HealthCheckOptions(
                        Predicate = (fun _ -> true),
                        ResponseWriter = Func<HttpContext, HealthReport, Task>(fun (context) (c: HealthReport) -> UIResponseWriter.WriteHealthCheckUIResponse(context, c))
                    )
                ) |> ignore
            ) |> ignore
