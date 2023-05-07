using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TemperatureViewer.Models.ViewModels
{
    [Keyless]
    public class SmtpSettings
    {
        [Required(ErrorMessage = "Поле {0} является обязательным.")]
        [DisplayName("Сервер")]
        public string Server { get; set; }
        [Required(ErrorMessage = "Поле {0} является обязательным.")]
        [EmailAddress]
        [DisplayName("Отправитель")]
        public string Sender { get; set; }

        public bool SSL { get; set; }
        [DisplayName("Порт")]
        [Range(1, ushort.MaxValue, ErrorMessage = "Поле должно содержать значение от 1 до 65535.")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Поле должно содержать значение от 1 до 65535.")]
        public int Port { get; set; }
        [Required(ErrorMessage = "Поле {0} является обязательным.")]
        [EmailAddress]
        [DisplayName("Логин")]
        public string Login { get; set; }
        [Required(ErrorMessage = "Поле {0} является обязательным.")]
        [DataType(DataType.Password)]
        [DisplayName("Пароль")]
        public string Password { get; set; }
    }
}
