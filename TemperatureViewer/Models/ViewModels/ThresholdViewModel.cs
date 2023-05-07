using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace TemperatureViewer.Models.ViewModels
{
    public class ThresholdViewModel
    {
        [RegularExpression(@"^[+-]?\d+$")]
        [Required(ErrorMessage = "Поле должно содержать целое число.")]
        public int P1 { get; set; }
        [RegularExpression(@"^[+-]?\d+$")]
        [Required(ErrorMessage = "Поле должно содержать целое число.")]
        public int P2 { get; set; }
        [RegularExpression(@"^[+-]?\d+$")]
        [Required(ErrorMessage = "Поле должно содержать целое число.")]
        public int P3 { get; set; }
        [Required(ErrorMessage = "Поле должно содержать целое число.")]
        [RegularExpression(@"^[+-]?\d+$")]
        public int P4 { get; set; }

        [DisplayName("Датчики")]
        public IDictionary<int, int> Sensors { get; set; }
        public ICollection<string> SensorNames { get; set; }
    }
}
