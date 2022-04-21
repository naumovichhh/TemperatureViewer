using System;
using System.Collections.Generic;

namespace TemperatureViewer.Models
{
    public class ExtendedLocationViewModel
    {
        public string Name { get; set; }
        public string Image { get; set; }
        public IList<SensorHistoryViewModel> History { get; set; }
        public IEnumerable<DateTime> HistoryCheckpoints { get; set; }
        public IEnumerable<MeasurementViewModel> Measurements { get; set; }
    }
}
