using System.Threading;
using System.Threading.Tasks;

namespace TemperatureViewer.BackgroundNAccessServices
{
    public interface ISingletonProcessingService
    {
        Task DoWorkAsync(CancellationToken token);
    }
}