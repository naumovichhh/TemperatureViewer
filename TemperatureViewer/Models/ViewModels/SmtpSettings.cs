using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TemperatureViewer.Models.ViewModels
{
    [Keyless]
    public class SmtpSettings
    {
        [Required]
        public string Server { get; set; }
        [Required]
        [EmailAddress]
        public string Sender { get; set; }
        [Required]
        [EmailAddress]
        public string Login { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
