﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly ISensorsRepository _sensorsRepository;

        public InformationService(ISensorsAccessService sensorsAccess, 
            IValuesRepository valuesRepository, 
            ILocationsRepository locationsRepository,
            ISensorsRepository sensorsRepository)
        {
            _sensorsAccess = sensorsAccess;
            _valuesRepository = valuesRepository;
            _locationsRepository = locationsRepository;
            _sensorsRepository = sensorsRepository;
        }

        public ValueDTO[] GetValues(string userName = "")
        {
            IList<Sensor> visibleSensors;
            if (userName == string.Empty)
                visibleSensors = _sensorsRepository.GetAllAsync().Result;
            else
                visibleSensors = _sensorsRepository.GetAllAsync(true).Result.Where(s => s.Users.Count(u => u.Name == userName) > 0).ToList();

            ValueDTO[] values = _sensorsAccess.GetValues().Where(v => v != null && visibleSensors.Count(vs => vs.Id == v.Sensor.Id) > 0).ToArray();
            foreach (var value in values)
            {
                var thresholds = value.Sensor.Threshold ?? GetDefaultThreshold();
                value.Thresholds = new int[] { thresholds.P1, thresholds.P2, thresholds.P3, thresholds.P4 };
            }

            return values;
        }

        public IList<IEnumerable<Value>> GetHistoryEnumerableListMax50(int? id,
            DateTime fromDate,
            DateTime toDate,
            out IEnumerable<DateTime> measurementTimes,
            string userName = "")
        {
            IDictionary<int, IEnumerable<Value>> dictionary;
            if (id != null)
            {

                IEnumerable<Value> enumerable = GetData(id.Value, fromDate, toDate, userName);
                measurementTimes = enumerable.Select(m => m.MeasurementTime);
                if (enumerable.Count() == 0)
                    dictionary = new Dictionary<int, IEnumerable<Value>>();
                else
                    dictionary = new Dictionary<int, IEnumerable<Value>>() { { id.Value, enumerable } };
            }
            else
            {
                dictionary = GetData(fromDate, toDate, out measurementTimes, userName);
                
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

        public byte[] DownloadExcel(DateTime from, DateTime to, int? id, int? locationId, string userName = "")
        {
            MemoryStream resultStream;

            if (locationId != null)
            {
                resultStream = DownloadLocation(from, to, locationId.Value, userName);
            }
            else if (id == null)
            {
                resultStream = DownloadAll(from, to, userName);
            }
            else
            {
                resultStream = DownloadSensor(from, to, id.Value, userName);
            }

            return resultStream.ToArray();
        }

        private MemoryStream DownloadLocation(DateTime from, DateTime to, int locationId, string userName = "")
        {
            IEnumerable<DateTime> measurementTimes;
            IDictionary<int, IEnumerable<Value>> data = GetData(from, to, out measurementTimes, locationId, userName);
            IDictionary<Sensor, IEnumerable<Value>> output = new Dictionary<Sensor, IEnumerable<Value>>();
            foreach (var kvp in data)
            {
                output.Add(_sensorsRepository.GetByIdAsync(kvp.Key).Result, kvp.Value);
            }

            return ExcelService.Create(output, measurementTimes);
        }

        private MemoryStream DownloadAll(DateTime from, DateTime to, string userName = "")
        {
            IEnumerable<DateTime> measurementTimes;
            IDictionary<int, IEnumerable<Value>> data = GetData(from, to, out measurementTimes, userName);
            IDictionary<Sensor, IEnumerable<Value>> output = new Dictionary<Sensor, IEnumerable<Value>>();
            foreach (var kvp in data)
            {
                output.Add(_sensorsRepository.GetByIdAsync(kvp.Key).Result, kvp.Value);
            }

            return ExcelService.Create(output, measurementTimes);
        }

        private MemoryStream DownloadSensor(DateTime from, DateTime to, int id, string userName = "")
        {
            IEnumerable<Value> enumerable = GetData(id, from, to, userName);
            IDictionary<Sensor, IEnumerable<Value>> output = new Dictionary<Sensor, IEnumerable<Value>>()
                {
                    {
                        _sensorsRepository.GetByIdAsync(id).Result,
                        enumerable
                    }
                };
            return ExcelService.Create(output, output.Values.First().Select(m => m.MeasurementTime));
        }

        public IList<IEnumerable<Value>> GetLocationHistoryEnumerableListMax50(int locationId, DateTime from, DateTime to, out IEnumerable<DateTime> checkpoints, string userName = "")
        {
            var historyDictionary = GetData(from, to, out checkpoints, locationId, userName);
            return GetValuesEnumerableList(historyDictionary, checkpoints, out checkpoints);
        }

        public IList<LocationDTO> GetValuesOnLocations(string userName = "")
        {
            IList<Sensor> visibleSensors;
            if (userName == string.Empty)
                visibleSensors = _sensorsRepository.GetAllAsync().Result;
            else
                visibleSensors = _sensorsRepository.GetAllAsync(true).Result.Where(s => s.Users.Count(u => u.Name == userName) > 0).ToList();

            var locations = _locationsRepository.GetAllAsync().Result.OrderBy(l => l.Name);
            var locationDTOs = new List<LocationDTO>();
            foreach (var location in locations)
            {
                var valueDTOsNoThresholds = _sensorsAccess
                    .GetValues(location.Id)
                    .Where(e => e != null && visibleSensors.Count(vs => vs.Id == e.Sensor.Id) > 0)
                    .OrderBy(e => e.Sensor.Name)
                    .ToList();
                var valueDTOs = valueDTOsNoThresholds?.Select(e =>
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

        public LocationDTO GetValuesOnLocation(int id, string userName = "")
        {
            IList<Sensor> visibleSensors;
            if (userName == string.Empty)
                visibleSensors = _sensorsRepository.GetAllAsync().Result;
            else
                visibleSensors = _sensorsRepository.GetAllAsync(true).Result.Where(s => s.Users.Count(u => u.Name == userName) > 0).ToList();

            var entity = _locationsRepository.GetByIdAsync(id).Result;
            var valueDTOsNoThresholds = _sensorsAccess.GetValues(id).Where(e => e != null && visibleSensors.Count(vs => vs.Id == e.Sensor.Id) > 0).OrderBy(e => e.Sensor.Name);
            var valueDTOs = valueDTOsNoThresholds?.Select(e =>
            {
                Threshold threshold = e.Sensor.Threshold ?? GetDefaultThreshold();
                return new ValueDTO()
                {
                    Temperature = e.Temperature,
                    Sensor = e.Sensor,
                    Thresholds = new int[] { threshold.P1, threshold.P2, threshold.P3, threshold.P4 }
                };
            });
            return new LocationDTO() { Id = id, Image = entity.Image, Name = entity.Name, Values = valueDTOs };
        }

        public IList<IEnumerable<Value>> GetHistoryEnumerableList(int? id, DateTime fromDate, DateTime toDate, out IEnumerable<DateTime> measurementTimes, string userName = "")
        {

            IDictionary<int, IEnumerable<Value>> dictionary;
            if (id != null)
            {
                IEnumerable<Value> enumerable = GetData(id.Value, fromDate, toDate, userName);
                measurementTimes = enumerable.Select(m => m.MeasurementTime);
                if (enumerable.Count() == 0)
                    dictionary = new Dictionary<int, IEnumerable<Value>>();
                else
                    dictionary = new Dictionary<int, IEnumerable<Value>>() { { id.Value, enumerable } };
            }
            else
            {
                dictionary = GetData(fromDate, toDate, out measurementTimes, userName);
            }

            if (!dictionary.All(e => e.Value.Count() == dictionary.First().Value.Count()))
                throw new ArgumentException("Data must contain enumerables of values of equal length.", nameof(dictionary));

            if (dictionary.Count() == 0)
                return null;

            List<IEnumerable<Value>> list = GetValuesEnumerableList(dictionary);
            return list;
        }

        private IDictionary<int, IEnumerable<Value>> GetData(DateTime fromDate, DateTime toDate, out IEnumerable<DateTime> measurementTimes, int locationId, string userName = "")
        {
            IList<Sensor> visibleSensors;
            if (userName == string.Empty)
                visibleSensors = _sensorsRepository.GetAllAsync().Result;
            else
                visibleSensors = _sensorsRepository.GetAllAsync(true).Result.Where(s => s.Users.Count(u => u.Name == userName) > 0).ToList();

            Dictionary<int, List<Value>> dictionary = new Dictionary<int, List<Value>>();
            var query = _valuesRepository.GetFilteredAsync(v => v.Sensor.LocationId == locationId && v.MeasurementTime < toDate && v.MeasurementTime > fromDate, true).Result
                .Where(v => visibleSensors.Count(vs => vs.Id == v.Sensor.Id) > 0);
            var groupedByTime = query.GroupBy(v => v.MeasurementTime).OrderBy(g => g.Key);
            var sensorIds = query.Select(m => m.SensorId).Distinct();
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

        private static List<IEnumerable<Value>> GetValuesEnumerableList(
            IDictionary<int, IEnumerable<Value>> dictionary, 
            IEnumerable<DateTime> checkpointsIn, 
            out IEnumerable<DateTime> checkpointsOut)
        {
            if (!dictionary.All(e => e.Value.Count() == dictionary.First().Value.Count()))
                throw new ArgumentException("Data must contain enumerables of values of equal length.", nameof(dictionary));

            if (dictionary.Count() == 0)
            {
                checkpointsOut = null;
                return null;
            }

            var maxValuesNum = 50;
            var valuesCount = dictionary.First().Value.Count();
            int divisor = (valuesCount - 1) / maxValuesNum + 1;
            var measurementTimes = checkpointsIn.Where((m, i) => i % divisor == 0);
            List<IEnumerable<Value>> list = GetValuesEnumerableList(dictionary, divisor);
            checkpointsOut = measurementTimes;
            return list;
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

        private IEnumerable<Value> GetData(int id, DateTime fromDate, DateTime toDate, string userName = "")
        {
            IList<Sensor> visibleSensors;
            if (userName == string.Empty)
                visibleSensors = _sensorsRepository.GetAllAsync().Result;
            else
                visibleSensors = _sensorsRepository.GetAllAsync(true).Result.Where(s => s.Users.Count(u => u.Name == userName) > 0).ToList();

            return _valuesRepository.GetFilteredAsync(m => m.SensorId == id && m.MeasurementTime < toDate && m.MeasurementTime > fromDate, true).Result.Where(v => visibleSensors.Count(vs => vs.Id == v.SensorId) > 0).OrderBy(v => v.MeasurementTime);
        }

        private IDictionary<int, IEnumerable<Value>> GetData(DateTime fromDate, DateTime toDate, out IEnumerable<DateTime> measurementTimes, string userName = "")
        {
            IList<Sensor> visibleSensors;
            if (userName == string.Empty)
                visibleSensors = _sensorsRepository.GetAllAsync().Result;
            else
                visibleSensors = _sensorsRepository.GetAllAsync(true).Result.Where(s => s.Users.Count(u => u.Name == userName) > 0).ToList();

            Dictionary<int, List<Value>> dictionary = new Dictionary<int, List<Value>>();
            var values = _valuesRepository.GetFilteredAsync(m => m.MeasurementTime < toDate && m.MeasurementTime > fromDate).Result
                .Where(v => visibleSensors.Count(vs => vs.Id == v.SensorId) > 0);
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
