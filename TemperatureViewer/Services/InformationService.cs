using System.Linq;
using TemperatureViewer.Models.DTO;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.BackgroundNAccessServices;

namespace TemperatureViewer.Services
{
    public class InformationService
    {
        private readonly ISensorsAccessService _sensorsAccess;

        public InformationService(ISensorsAccessService sensorsAccess)
        {
            _sensorsAccess = sensorsAccess;
        }

        public ValueDTO[] GetValues()
        {
            ValueDTO[] values = _sensorsAccess.GetValues().Where(v => v != null).ToArray();
            foreach (var value in values)
            {
                var thresholds = value.Sensor.Threshold ?? GetDefaultThreshold();
                value.Thresholds = new int[] { thresholds.P1, thresholds.P2, thresholds.P3, thresholds.P4 };
            }

            return values;
        }

        public static Threshold GetDefaultThreshold() => new Threshold() { P1 = 12, P2 = 16, P3 = 25, P4 = 30 };
    }
}
