using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureViewer.Models
{
    public class Termometer
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Measurement> Measurements { get; set; }
    }
}
