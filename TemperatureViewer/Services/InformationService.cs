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
        private readonly ILocationsRepository _locationsRepository;
        //private readonly DefaultContext _context;

        public InformationService(ISensorsAccessService sensorsAccess, 
            IValuesRepository valuesRepository, 
            ILocationsRepository locationsRepository)
        {
            _sensorsAccess = sensorsAccess;
            _valuesRepository = valuesRepository;
            _locationsRepository = locationsRepository;
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

        public IList<IEnumerable<Value>> GetHistoryEnumerableList(int? id, DateTime fromDate, DateTime toDate, out IEnumerable<DateTime> measurementTimes)
        {

            IDictionary<int, IEnumerable<Value>> dictionary;
            if (id != null)
            {
                IEnumerable<Value> enumerable = GetData(id.Value, fromDate, toDate);
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
            return list;
        }

        public IList<IEnumerable<Value>> GetHistoryEnumerableListMax50(int? id, DateTime fromDate, DateTime toDate, out IEnumerable<DateTime> measurementTimes)
        {
            IDictionary<int, IEnumerable<Value>> dictionary;
            if (id != null)
            {

                IEnumerable<Value> enumerable = GetData(id.Value, fromDate, toDate);
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

            var maxValuesNum = 50;
            var valuesCount = dictionary.First().Value.Count();
            int divisor = (valuesCount - 1) / maxValuesNum + 1;
            measurementTimes = measurementTimes.Where((m, i) => i % divisor == 0);

            List<IEnumerable<Value>> list = GetValuesEnumerableList(dictionary, divisor);
            return list;
        }

        public IList<LocationDTO> GetValuesOnLocations()
        {
            var locations = _locationsRepository.GetAllAsync().Result.OrderBy(l => l.Name);
            var locationDTOs = new List<LocationDTO>();
            foreach (var location in locations)
            {
                var values = _sensorsAccess.GetValues(location.Id).Where(e => e != null).OrderBy(e => e.Sensor.Name).ToList();
                var valueDTOs = values?.Select(e =>
                {
                    Threshold threshold = e.Sensor.Threshold ?? GetDefaultThreshold();
                    return new ValueDTO()
                    {
                        Temperature = e.Temperature,
                        Sensor = e.Sensor,
                        Thresholds = new int[] { threshold.P1, threshold.P2, threshold.P3, threshold.P4 }
                    };
                });

                locationDTOs.Add(new LocationDTO() { Id = location.Id, Name = location.Name, Image = location.Image, Values = valueDTOs });
            }

            return locationDTOs;
        }

        public List<LocationDTO> GetValuesOnLocation(int id)
        {
            var values = _sensorsAccess.GetValues(id).Where(e => e != null).OrderBy(e => e.Sensor.Name);
            return values.Select(v => v.);
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

        private IEnumerable<Value> GetData(int id, DateTime fromDate, DateTime toDate)
        {
            return _valuesRepository.GetFilteredAsync(m => m.SensorId == id && m.MeasurementTime < toDate && m.MeasurementTime > fromDate).Result.OrderBy(v => v.MeasurementTime);
            //return _context.Values.AsNoTracking().Where(m => m.SensorId == id && m.MeasurementTime < toDate && m.MeasurementTime > fromDate).OrderBy(m => m.MeasurementTime).AsEnumerable();
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
