using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TemperatureViewer.Models.ViewModels
{
    public class UserViewModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Поле '{0}' обязательно.")]
        [DisplayName("Имя")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Поле '{0}' обязательно.")]
        [DataType(DataType.Password)]
        [DisplayName("Пароль")]
        public string Password { get; set; }
        [DisplayName("Роль")]
        public string Role { get; set; }
        [DisplayName("Датчики")]
        public IDictionary<int, int> Sensors { get; set; }
    }
}
