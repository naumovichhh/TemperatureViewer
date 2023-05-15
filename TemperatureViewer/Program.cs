using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using TemperatureViewer.BackgroundNAccessServices;
using TemperatureViewer.Database;

namespace TemperatureViewer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            CreateDbIfNotExists(host);
            host.Run();
        }

        private static void CreateDbIfNotExists(IHost host)
        {
            DbInitializer.Initialize(host);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ISingletonProcessingService, SensorsAccessService>();
                    services.AddSingleton<ISensorsAccessService>(p => (SensorsAccessService)p.GetService<ISingletonProcessingService>());
                    services.AddHostedService<DefaultBackgroundService>();
                });
    }
}
