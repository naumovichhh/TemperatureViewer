using TemperatureViewer.Models;

namespace TemperatureViewer.BackgroundServices
{
    public interface ISensorsAccessService
    {
        void AddSensor(Sensor sensor);
        void RemoveSensor(Sensor sensor);
        Measurement[] GetMeasurements();
    }
}