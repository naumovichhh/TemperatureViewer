﻿using System.ComponentModel.DataAnnotations;

namespace TemperatureViewer.Models
{
    public class Location
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Image { get; set; }
    }
}