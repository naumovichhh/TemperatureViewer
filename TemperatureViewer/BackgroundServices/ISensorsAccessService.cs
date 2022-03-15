using TemperatureViewer.Models;

namespace TemperatureViewer.BackgroundServices
{
    public interface ISensorsAccessService
    {
        Measurement[] GetMeasurements();
    }
}