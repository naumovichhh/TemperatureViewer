using System.Collections.Generic;

namespace TemperatureViewer.Models
{
    public class ThresholdViewModel
    {
        public int P1 { get; set; }
        public int P2 { get; set; }
        public int P3 { get; set; }
        public int P4 { get; set; }

        public IDictionary<int, int> Sensors { get; set; }
    }
}
