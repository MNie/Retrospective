using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebApp
{
    using System;
    using System.Net;
    using Ably.HealthCheck;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Configuration;
    using HealthChecks.UI.Client;
    using IO.Ably;
    using Microsoft.AspNetCore.Diagnostics.HealthChecks;
    using Microsoft.FSharp.Core;
    using Microsoft.OpenApi.Models;

    public class Startup
    {
        private readonly IConfiguration _configuration;
        public Startup(IConfiguration configuration) => _configuration = configuration;

        public IConfiguration Configuration => _configuration;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = Configuration
                .GetSection("Ably")
                .Get<AblyConfig>();
            var ably = new AblyRealtime(config.ApiKey);
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "WebApp API", Version = "v1"});
            });
            services.AddHealthChecks()
                .AddCheck(
                    "Ably Channel",
                    new AblyChannelHealthCheck(
                        ably,
                        "Topic"
                    )
                )
                .AddCheck(
                    "Ably Timer",
                    new AblyTimerHealthCheck(
                        ably,
                        "Topic",
                        FSharpOption<TimeSpan>.Some(TimeSpan.FromSeconds(1)),
                        FSharpOption<TimeSpan>.Some(TimeSpan.FromSeconds(1))
                    )
                );
            services
                .AddHealthChecksUI(s =>
                        s
                    .SetEvaluationTimeInSeconds(60)
                    .AddHealthCheckEndpoint("Self", $"http://{Dns.GetHostName()}/health"))
                .AddInMemoryStorage();
            services.AddControllersWithViews();
            services.AddAutofac();
            services.AddRouting();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration => { configuration.RootPath = "ClientApp/dist"; });
        }
        
        public void ConfigureContainer(ContainerBuilder builder) =>
            builder.RegisterModule(new WebAppModule(Configuration
                .GetSection("Ably")
                .Get<AblyConfig>()));

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Dashboard API V1");
            });

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapHealthChecksUI(setup =>
                        {
                            setup.UIPath = "/ui-health";
                            setup.ApiPath = "/api-ui-health";
                        }
                    );
                endpoints.MapHealthChecks(
                    "/health",
                    new HealthCheckOptions {
                        Predicate = (_ => true),
                        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                    });
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}