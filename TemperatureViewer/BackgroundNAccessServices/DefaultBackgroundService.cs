using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TemperatureViewer.BackgroundNAccessServices
{
    public class DefaultBackgroundService : BackgroundService
    {
        private IServiceProvider serviceProvider;

        public DefaultBackgroundService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return DoWorkAsync(stoppingToken);
        }

        private Task DoWorkAsync(CancellationToken stoppingToken)
        {
            ISingletonProcessingService service = serviceProvider.GetRequiredService<ISingletonProcessingService>();
            return service.DoWorkAsync(stoppingToken);
        }
    }
}
