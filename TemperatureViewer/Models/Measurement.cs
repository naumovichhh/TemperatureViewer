using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureViewer.Models
{
    public class Measurement
    {
        public int Id { get; set; }
        public int TermometerId { get; set; }
        public DateTime MeasurementTime { get; set; }
        public decimal Temperature { get; set; }

        public Termometer Termometer { get; set; }
    }
}
