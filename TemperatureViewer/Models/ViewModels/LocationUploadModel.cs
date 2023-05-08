using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TemperatureViewer.Models.ViewModels
{
    public class LocationUploadModel
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(30, ErrorMessage = "Максимальная длина названия - 30.")]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.Upload)]
        public IFormFile Image { get; set; }
    }
}
