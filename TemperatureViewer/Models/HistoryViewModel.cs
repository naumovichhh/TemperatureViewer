using System;
using System.Collections.Generic;

namespace TemperatureViewer.Models
{
    public class SensorHistoryViewModel
    {
        public string SensorName { get; set; }
        public IEnumerable<MeasurementOfTime> Measurements { get; set; }
    }

    public class MeasurementOfTime
    {
        public DateTime Time { get; set; }
        public decimal Value { get; set; }
    }
}
