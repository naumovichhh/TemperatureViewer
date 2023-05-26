using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TemperatureViewer.Models.ViewModels
{
    public class ObserverViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле обязательно для заполнения.")]
        [EmailAddress(ErrorMessage = "Некорректный адрес электронной почты.")]
        [MaxLength(40, ErrorMessage = "Максимальная длина адреса - 40.")]
        public string Email { get; set; }
        [DisplayName("Датчики")]
        public IDictionary<int, int> Sensors { get; set; }
        public IDictionary<int, bool> SensorsFlags { get; set; }
    }
}
