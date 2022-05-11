using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TemperatureViewer.Models.ViewModels
{
    public class ObserverViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public IDictionary<int, int> Sensors { get; set; }
    }
}
