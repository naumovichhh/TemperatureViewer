using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TemperatureViewer.Models.Entities
{
    public class Location
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Название")]
        [MaxLength(30)]
        public string Name { get; set; }
        [Required]
        [DisplayName("Изображение")]
        public string Image { get; set; }

        public ICollection<Sensor> Sensors { get; set; }
    }
}
