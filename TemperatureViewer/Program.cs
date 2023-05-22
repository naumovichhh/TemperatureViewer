using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using TemperatureViewer.BackgroundNAccessServices;
using TemperatureViewer.Database;

namespace TemperatureViewer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().LogFactory.GetCurrentClassLogger();

            try
            {
                logger.Debug("Run host");
                var host = CreateHostBuilder(args).Build();
                CreateDbIfNotExists(host);
                host.Run();
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown();
            }
        }

        private static void CreateDbIfNotExists(IHost host)
        {
            DbInitializer.Initialize(host);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .ConfigureServices(services =>
                {
                    services.AddSingleton<ISingletonProcessingService, SensorsAccessService>();
                    services.AddSingleton<ISensorsAccessService>(p => (SensorsAccessService)p.GetService<ISingletonProcessingService>());
                    services.AddHostedService<DefaultBackgroundService>();
                })
                .UseNLog();
    }
}
