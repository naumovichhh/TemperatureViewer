using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TemperatureViewer.Models.Entities
{
    public class Sensor
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Название")]
        public string Name { get; set; }
        [Required]
        public string Uri { get; set; }
        [DisplayName("Локация")]
        public int? LocationId { get; set; }
        [Required]
        [DisplayName("Отключен")]
        public bool WasDisabled { get; set; }
        public string XPath { get; set; }
        [DisplayName("Регулярное выражение")]
        public string Regex { get; set; }
        public int? ThresholdId { get; set; }

        public ICollection<Value> Values { get; set; }
        public ICollection<Observer> Observers { get; set; }
        public ICollection<User> Users { get; set; }
        public Threshold Threshold { get; set; }
        public Location Location { get; set; }
    }
}
