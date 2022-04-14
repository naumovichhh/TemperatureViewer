using System.Collections.Generic;

namespace TemperatureViewer.Models
{
    public class LocationViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }

        public IEnumerable<MeasurementViewModel> Measurements { get; set; }
    }
}
