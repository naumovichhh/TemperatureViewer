using TemperatureViewer.Models.Entities;

namespace TemperatureViewer.BackgroundServices
{
    public interface ISensorsAccessService
    {
        Measurement[] GetMeasurements();
        Measurement[] GetMeasurements(int locationId);
    }
}