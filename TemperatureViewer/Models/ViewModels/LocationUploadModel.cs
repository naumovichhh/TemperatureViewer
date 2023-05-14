using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TemperatureViewer.Models.ViewModels
{
    public class LocationUploadModel
    {
        public int Id { get; set; }
        [DisplayName("Название")]
        [Required]
        [MaxLength(30, ErrorMessage = "Максимальная длина названия - 30.")]
        public string Name { get; set; }
        [DisplayName("Изображение")]
        [Required]
        [DataType(DataType.Upload)]
        public IFormFile Image { get; set; }
    }
}
