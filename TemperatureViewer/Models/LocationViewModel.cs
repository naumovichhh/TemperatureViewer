using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TemperatureViewer.Models
{
    public class LocationViewModel
    {
        public string Name { get; set; }
        [Required]
        [DataType(DataType.Upload)]
        public IFormFile Image { get; set; }
    }
}
