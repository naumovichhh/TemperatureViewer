namespace TemperatureViewer.Models.ViewModels
{
    public class ValueViewModel
    {
        public decimal Temperature { get; set; }
        public string SensorName { get; set; }
        public int SensorId { get; set; }
        public int[] Thresholds { get; set; }
    }
}
