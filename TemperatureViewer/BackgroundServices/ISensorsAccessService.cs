using TemperatureViewer.Models.Entities;

namespace TemperatureViewer.BackgroundServices
{
    public interface ISensorsAccessService
    {
        Value[] GetValues();
        Value[] GetValues(int locationId);
        Value[] GetValues(int[] sensorIds);
    }
}