namespace WebApi

open System.Net
open System.Text.Json.Serialization
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
open System

open WebApi.Configuration

type Startup(configuration: IConfiguration) =
    let corsPolicy = "defaultCorsPolicy"

    member _.Configuration = configuration

    member private this.ConfigureHandlers (ably: AblyRealtime) (config: AblyConfig) (logger: ILogger) =
        let channel = ably.Channels.Get config.Channels.Notification.Name
        channel.Subscribe(
            config.Channels.Notification.MessageType,
            fun msg -> WebApi.Notifications.Save.handle ably config msg.Data logger |> Async.RunSynchronously)
    
    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        let config = Config ()
        this.Configuration.Bind config
        let ably = new AblyRealtime (config.Ably.ApiKey)
        services.AddSingleton<AblyRealtime>(fun _ -> ably) |> ignore
        services.AddSingleton<Config> (fun _ -> config) |> ignore
        
        let loggerFactory =
             LoggerFactory.Create(fun builder ->
                builder.AddConsole() |> ignore               
             );
        let logger = loggerFactory.CreateLogger();
        this.ConfigureHandlers ably config.Ably logger
        
        services.AddSwaggerGen(fun c ->
                c.SwaggerDoc("v1", OpenApiInfo(Title = "Notification Api", Version = "v1"))
        ) |> ignore
        
        services.AddHealthChecks()
            .AddCheck(
                "Ably Channel",
                AblyChannelHealthCheck(
                    ably,
                    "Notification"
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
            ) |> ignore
        services
            .AddHealthChecksUI(fun s ->
                s
                    .SetEvaluationTimeInSeconds(60)
                    .AddHealthCheckEndpoint(
                        "Self",
                        $"http://{Dns.GetHostName()}/health"
                    ) |> ignore)
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
                opt.SwaggerEndpoint("/swagger/v1/swagger.json", "Notification Api V1")
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

