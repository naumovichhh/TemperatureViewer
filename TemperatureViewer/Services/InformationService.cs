using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TemperatureViewer.Models.DTO;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.BackgroundNAccessServices;
using TemperatureViewer.Repositories;

namespace TemperatureViewer.Services
{
    public class InformationService
    {
        private readonly ISensorsAccessService _sensorsAccess;
        private readonly IValuesRepository _valuesRepository;
        //private readonly DefaultContext _context;

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
        private async Task<IEnumerable<Value>> GetDataAsync(int id, DateTime fromDate, DateTime toDate)
        {
            return (await _valuesRepository.GetFilteredAsync(m => m.SensorId == id && m.MeasurementTime < toDate && m.MeasurementTime > fromDate))
                .OrderBy(v => v.MeasurementTime);
            //return _context.Values.AsNoTracking().Where(m => m.SensorId == id && m.MeasurementTime < toDate && m.MeasurementTime > fromDate).OrderBy(m => m.MeasurementTime).AsEnumerable();
        }

        public IList<IEnumerable<Value>> GetHistoryEnumerableList(int? id, DateTime fromDate, DateTime toDate, out IEnumerable<DateTime> measurementTimes)
        {

            IDictionary<int, IEnumerable<Value>> dictionary;
            if (id != null)
            {
                IEnumerable<Value> enumerable = GetDataAsync(id.Value, fromDate, toDate).Result;
                measurementTimes = enumerable.Select(m => m.MeasurementTime);
                if (enumerable.Count() == 0)
                    dictionary = new Dictionary<int, IEnumerable<Value>>();
                else
                    dictionary = new Dictionary<int, IEnumerable<Value>>() { { id.Value, enumerable } };
            }
            else
            {
                dictionary = GetData(fromDate, toDate, out measurementTimes);
            }

            if (!dictionary.All(e => e.Value.Count() == dictionary.First().Value.Count()))
                throw new ArgumentException("Data must contain enumerables of values of equal length.", nameof(dictionary));

            if (dictionary.Count() == 0)
                return null;

            List<IEnumerable<Value>> list = GetValuesEnumerableList(dictionary);
        }

        private static List<IEnumerable<Value>> GetValuesEnumerableList(IDictionary<int, IEnumerable<Value>> dictionary)
        {
            List<IEnumerable<Value>> list = new List<IEnumerable<Value>>();
            foreach (var keyValuePair in dictionary)
            {
                list.Add(keyValuePair.Value);
            }

            return list;
        }

        private static List<IEnumerable<Value>> GetValuesEnumerableList(IDictionary<int, IEnumerable<Value>> dictionary, int divisor)
        {
            List<IEnumerable<Value>> list = new List<IEnumerable<Value>>();
            foreach (var keyValuePair in dictionary)
            {
                list.Add(keyValuePair.Value.Where((m, i) => i % divisor == 0));
            }

            return list;
        }

        private static List<IEnumerable<Value>> GetValuesEnumerableList(IEnumerable<IGrouping<int, Value>> groups, int[] offsets, int divisor)
        {
            List<IEnumerable<Value>> list = new List<IEnumerable<Value>>();
            int j = 0;
            foreach (var group in groups)
            {
                int k = j;
                list.Add(group.OrderBy(m => m.MeasurementTime).Where((m, i) => (i + offsets[k]) % divisor == 0));
                j++;
            }

            return list;
        }

        private IDictionary<int, IEnumerable<Value>> GetData(DateTime fromDate, DateTime toDate, out IEnumerable<DateTime> measurementTimes)
        {
            Dictionary<int, List<Value>> dictionary = new Dictionary<int, List<Value>>();
            var values = _valuesRepository.GetFilteredAsync(m => m.MeasurementTime < toDate && m.MeasurementTime > fromDate).Result;
            var groupedByTime = values.GroupBy(m => m.MeasurementTime).OrderBy(g => g.Key);
            var sensorIds = values.Select(m => m.SensorId).Distinct();
            foreach (var sensorId in sensorIds)
            {
                dictionary.Add(sensorId, new List<Value>());
            }

            measurementTimes = groupedByTime.Select(g => g.Key);

            foreach (var valuesInTime in groupedByTime)
            {
                foreach (var keyValuePair in dictionary)
                {
                    if (valuesInTime.Where(m => m.SensorId == keyValuePair.Key).Count() > 0)
                    {
                        keyValuePair.Value.Add(valuesInTime.First(m => m.SensorId == keyValuePair.Key));
                    }
                    else
                    {
                        keyValuePair.Value.Add(null);
                    }
                }
            }

            Dictionary<int, IEnumerable<Value>> result = new Dictionary<int, IEnumerable<Value>>();
            foreach (var keyValuePair in dictionary)
            {
                result.Add(keyValuePair.Key, keyValuePair.Value);
            }
            return result;
        }

        public static Threshold GetDefaultThreshold() => new Threshold() { P1 = 12, P2 = 16, P3 = 25, P4 = 30 };
    }
}
