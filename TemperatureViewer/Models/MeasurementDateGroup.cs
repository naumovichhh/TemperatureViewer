using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TemperatureViewer.Models
{
    public class MeasurementDateGroup
    {
        [DataType(DataType.Date)]
        public DateTime? MeasurementDate { get; set; }

        public int MeasurementsCount { get; set; }
    }
}
