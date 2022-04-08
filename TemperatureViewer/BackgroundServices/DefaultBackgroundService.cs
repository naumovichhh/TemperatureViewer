using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace TemperatureViewer.BackgroundServices
{
    public class DefaultBackgroundService : Microsoft.Extensions.Hosting.BackgroundService
    {
        private IServiceProvider serviceProvider;

        public DefaultBackgroundService(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            ISingletonProcessingService service = serviceProvider.GetRequiredService<ISingletonProcessingService>();
            await service.DoWork(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await base.StopAsync(stoppingToken);
        }
    }
}
