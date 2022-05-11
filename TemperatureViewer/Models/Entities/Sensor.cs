using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TemperatureViewer.Models.Entities
{
    public class Sensor
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Uri { get; set; }
        public int? LocationId { get; set; }
        [Required]
        public bool WasDeleted { get; set; }
        public string XPath { get; set; }
        public int? ThresholdId { get; set; }

        public ICollection<Measurement> Measurements { get; set; }
        public ICollection<Observer> Observers { get; set; }
        public Threshold Threshold { get; set; }
        public Location Location { get; set; }
    }
}
