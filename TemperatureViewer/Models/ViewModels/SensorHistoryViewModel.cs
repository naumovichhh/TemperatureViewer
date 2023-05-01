using System;
using System.Collections.Generic;

namespace TemperatureViewer.Models.ViewModels
{
    public class SensorHistoryViewModel
    {
        public string SensorName { get; set; }
        public IEnumerable<ValueOfTime> Values { get; set; }
    }

    public class ValueOfTime
    {
        public DateTime Time { get; set; }
        public decimal Value { get; set; }
    }
}
