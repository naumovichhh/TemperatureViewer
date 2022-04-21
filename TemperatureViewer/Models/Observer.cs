using System.ComponentModel.DataAnnotations;

namespace TemperatureViewer.Models
{
    public class Observer
    {
        [Key]
        public string Email { get; set; }
    }
}
