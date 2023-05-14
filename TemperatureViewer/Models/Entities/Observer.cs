using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TemperatureViewer.Models.Entities
{
    public class Observer
    {
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        public ICollection<Sensor> Sensors { get; set; }
    }
}
