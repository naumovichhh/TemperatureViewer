using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TemperatureViewer.Models
{
    public class Sensor
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Uri { get; set; }
        public int? LocationId { get; set; }

        public ICollection<Measurement> Measurements { get; set; }
        public Location Location { get; set; }
    }
}
