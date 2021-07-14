using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApp
{
    using Autofac.Extensions.DependencyInjection;

    public class Program
    {
        public static Task Main(string[] args) =>
            CreateWebHostBuilder(args).Build().RunAsync();

        private static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(wb =>
                {
                    wb.UseStartup<Startup>();
                    wb.ConfigureLogging((_, lg) =>
                    {
                        lg.ClearProviders();
                        lg.AddDebug();
                        lg.AddConsole();
                    });
                });
    }
}