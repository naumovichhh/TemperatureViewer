using TemperatureViewer.Models.DTO;

namespace TemperatureViewer.BackgroundNAccessServices
{
    public interface ISensorsAccessService
    {
        ValueDTO[] GetValues(bool slowly = false);
        ValueDTO[] GetValues(int locationId, bool slowly = false);
        ValueDTO[] GetValues(int[] sensorIds, bool slowly = false);
    }
}