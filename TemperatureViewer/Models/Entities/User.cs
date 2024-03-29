﻿using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TemperatureViewer.Models.Entities
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Имя")]
        public string Name { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public string Role { get; set; }
        public ICollection<Sensor> Sensors { get; set; }
    }
}
