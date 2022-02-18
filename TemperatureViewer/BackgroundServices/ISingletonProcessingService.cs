using System.Threading;
using System.Threading.Tasks;

namespace TemperatureViewer.BackgroundServices
{
    public interface ISingletonProcessingService
    {
        Task DoWork(CancellationToken token);
    }
}