﻿using System;

namespace TemperatureViewer.Models.Entities
{
    public class Value
    {
        public int Id { get; set; }
        public int SensorId { get; set; }
        public DateTime MeasurementTime { get; set; }
        public decimal Temperature { get; set; }

        public Sensor Sensor { get; set; }
    }
}
