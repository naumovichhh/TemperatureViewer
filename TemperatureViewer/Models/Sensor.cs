using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureViewer.Models
{
    public class Sensor
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Uri { get; set; }

        public ICollection<Measurement> Measurements { get; set; }
    }
}
