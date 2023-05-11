using TemperatureViewer.Models.Entities;

namespace TemperatureViewer.Models.DTO
{
    public class ValueDTO
    {
        public decimal? Temperature { get; set; }
        public Sensor Sensor { get; set; }
    }
}
