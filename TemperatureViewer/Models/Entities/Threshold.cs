﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TemperatureViewer.Models.Entities
{
    public class Threshold
    {
        public int Id { get; set; }
        public int P1 { get; set; }
        public int P2 { get; set; }
        public int P3 { get; set; }
        public int P4 { get; set; }

        public ICollection<Sensor> Sensors { get; set; }
    }
}
