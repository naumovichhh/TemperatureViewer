using System;
using System.ComponentModel.DataAnnotations;

namespace TemperatureViewer.Models
{
    public class MeasurementDateGroup
    {
        [DataType(DataType.Date)]
        public DateTime? MeasurementDate { get; set; }

        public int MeasurementsCount { get; set; }
    }
}
