using TemperatureViewer.Models.DTO;

namespace TemperatureViewer.BackgroundServices
{
    public interface ISensorsAccessService
    {
        ValueDTO[] GetValues();
        ValueDTO[] GetValues(int locationId);
        ValueDTO[] GetValues(int[] sensorIds);
    }
}