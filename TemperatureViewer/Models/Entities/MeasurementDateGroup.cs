using System;
using System.ComponentModel.DataAnnotations;

namespace TemperatureViewer.Models.Entities
{
    public class MeasurementDateGroup
    {
        [DataType(DataType.Date)]
        public DateTime? MeasurementDate { get; set; }

        public int MeasurementsCount { get; set; }
    }
}
