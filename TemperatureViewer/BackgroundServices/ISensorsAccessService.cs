using TemperatureViewer.Models;

namespace TemperatureViewer.BackgroundServices
{
    public interface ISensorsAccessService
    {
        Measurement[] GetMeasurements();
        Measurement[] GetMeasurements(int locationId);
    }
}