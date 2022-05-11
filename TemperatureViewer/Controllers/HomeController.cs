using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TemperatureViewer.BackgroundServices;
using TemperatureViewer.Data;
using TemperatureViewer.Helpers;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Models.ViewModels;

namespace TemperatureViewer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private DefaultContext _context;
        private ISensorsAccessService _sensorsAccessService;

        public HomeController(ILogger<HomeController> logger, DefaultContext context, ISensorsAccessService temperatureService)
        {
            _logger = logger;
            _context = context;
            _sensorsAccessService = temperatureService;
        }

        public IActionResult LocationDropdown()
        {
            var locations = _context.Locations.AsNoTracking().OrderBy(l => l.Name).AsEnumerable();
            return PartialView(locations);
        }

        public IActionResult Index(int? locationId)
        {
            IEnumerable<Measurement> measurements;
            if (locationId != null)
            {
                if (_context.Locations.FirstOrDefault(l => l.Id == locationId) == null)
                {
                    return NotFound();
                }

                measurements = _sensorsAccessService.GetMeasurements(locationId.Value);
                ViewBag.location = _context.Locations.FirstOrDefault(l => l.Id == locationId);
            }
            else
            {
                measurements = _sensorsAccessService.GetMeasurements();
                ViewBag.location = null;
            }

            var viewModel = measurements?.Select(e =>
            {
                Threshold threshold = e.Sensor.Threshold ?? GetDefaultThreshold();
                return new MeasurementViewModel()
                {
                    Temperature = e.Temperature,
                    SensorName = e.Sensor.Name,
                    SensorId = e.Sensor.Id,
                    Thresholds = new int[] { threshold.P1, threshold.P2, threshold.P3, threshold.P4 }
                };
            });
            return View(viewModel);
        }

        public IActionResult History(int? id, string from, string to, int? locationId)
        {
            DateTime fromDate, toDate;
            fromDate = GetFromDateTime(from);
            toDate = GetToDateTime(to);
            ViewBag.from = from;
            ViewBag.to = to;
            ViewBag.allSensors = false;
            IEnumerable<DateTime> measurementTimes;

            IDictionary<int, IEnumerable<Measurement>> dictionary;
            if (locationId != null)
            {
                if (_context.Locations.FirstOrDefault(l => l.Id == locationId) == null)
                {
                    return NotFound();
                }

                dictionary = GetData(fromDate, toDate, out measurementTimes, locationId.Value);
                ViewBag.locationId = locationId;
            }
            else if (id != null)
            {
                if (_context.Sensors.FirstOrDefault(s => s.Id == id) == null)
                {
                    return NotFound();
                }

                IEnumerable<Measurement> enumerable = GetData(id.Value, fromDate, toDate);
                measurementTimes = enumerable.Select(m => m.MeasurementTime);
                if (enumerable.Count() == 0)
                    dictionary = new Dictionary<int, IEnumerable<Measurement>>();
                else
                    dictionary = new Dictionary<int, IEnumerable<Measurement>>() { { id.Value, enumerable } };
                ViewBag.id = id;
            }
            else
            {
                dictionary = GetData(fromDate, toDate, out measurementTimes);
                ViewBag.allSensors = true;
            }

            if (!dictionary.All(e => e.Value.Count() == dictionary.First().Value.Count()))
                throw new ArgumentException("Data must contain enumerables of measurements of equal length.", nameof(dictionary));

            if (dictionary.Count() == 0)
                return View();

            var maxMeasurementsNum = 50;
            var measurementsCount = dictionary.First().Value.Count();
            int divisor = (measurementsCount - 1) / maxMeasurementsNum + 1;
            measurementTimes = measurementTimes.Where((m, i) => i % divisor == 0);

            List<IEnumerable<Measurement>> list = GetMeasurementsEnumerableList(dictionary, divisor);

            List<SensorHistoryViewModel> model = list.Select(GetViewModelsFromEnumerable).OrderBy(vm => vm.SensorName).ToList();
            ViewBag.measurementTimes = measurementTimes;
            ViewBag.location = _context.Locations.AsNoTracking().FirstOrDefault(l => l.Id == locationId);
            return View(model);
        }

        private SensorHistoryViewModel GetViewModelsFromEnumerable(IEnumerable<Measurement> enumerable)
        {
            return new SensorHistoryViewModel()
            {
                SensorName = _context.Sensors.AsNoTracking().AsEnumerable().First(s => s.Id == enumerable.First(m => m != null).SensorId).Name,
                Measurements = enumerable.Select(
                m =>
                {
                    if (m != null)
                        return new MeasurementOfTime() { Value = m.Temperature, Time = m.MeasurementTime };
                    else
                        return null;
                })
            };
        }

        public IActionResult Locations()
        {
            var locations = _context.Locations.OrderBy(l => l.Name).AsNoTracking().AsEnumerable();
            var viewModel = new List<LocationViewModel>();
            foreach (var location in locations)
            {
                var measurements = _sensorsAccessService.GetMeasurements(location.Id).OrderBy(e => e.Sensor.Name);
                var measurementsViewModels = measurements?.Select(e =>
                {
                    Threshold threshold = e.Sensor.Threshold ?? GetDefaultThreshold();
                    return new MeasurementViewModel()
                    {
                        Temperature = e.Temperature,
                        SensorName = e.Sensor.Name,
                        SensorId = e.Sensor.Id,
                        Thresholds = new int[] { threshold.P1, threshold.P2, threshold.P3, threshold.P4 }
                    };
                });

                viewModel.Add(new LocationViewModel() { Id = location.Id, Name = location.Name, Image = location.Image, Measurements = measurementsViewModels });
            }

            return View(viewModel);
        }

        public static Threshold GetDefaultThreshold() => new Threshold() { P1 = 12, P2 = 16, P3 = 25, P4 = 30 };

        public IActionResult ExtendedLocation(int? id, string from, string to)
        {
            if (id == null)
            {
                return NotFound();
            }

            Location entity;
            if ((entity = _context.Locations.AsNoTracking().FirstOrDefault(l => l.Id == id)) == null)
            {
                return NotFound();
            }

            DateTime fromDate, toDate;
            fromDate = GetFromDateTime(from);
            toDate = GetToDateTime(to);
            ViewBag.from = from;
            ViewBag.to = to;
            IEnumerable<DateTime> checkpoints;

            var historyDictionary = GetData(fromDate, toDate, out checkpoints, id.Value);
            IList<SensorHistoryViewModel> history = GetHistoryViewModel(historyDictionary, checkpoints, out checkpoints);

            var measurements = _sensorsAccessService.GetMeasurements(entity.Id).OrderBy(e => e.Sensor.Name);
            var measurementsViewModels = measurements?.Select(e =>
            {
                Threshold threshold = e.Sensor.Threshold ?? new Threshold() { P1 = 12, P2 = 16, P3 = 25, P4 = 30 };
                return new MeasurementViewModel()
                {
                    Temperature = e.Temperature,
                    SensorName = e.Sensor.Name,
                    SensorId = e.Sensor.Id,
                    Thresholds = new int[] { threshold.P1, threshold.P2, threshold.P3, threshold.P4 }
                };
            });

            ExtendedLocationViewModel viewModel = new ExtendedLocationViewModel()
            {
                Name = entity.Name,
                Image = entity.Image,
                History = history,
                HistoryCheckpoints = checkpoints,
                Measurements = measurementsViewModels
            };

            ViewBag.locationId = id;
            return View(viewModel);
        }

        private IList<SensorHistoryViewModel> GetHistoryViewModel(IDictionary<int, IEnumerable<Measurement>> dictionary, IEnumerable<DateTime> checkpointsIn, out IEnumerable<DateTime> checkpointsOut)
        {
            if (!dictionary.All(e => e.Value.Count() == dictionary.First().Value.Count()))
                throw new ArgumentException("Data must contain enumerables of measurements of equal length.", nameof(dictionary));

            if (dictionary.Count() == 0)
            {
                checkpointsOut = null;
                return null;
            }

            var maxMeasurementsNum = 50;
            var measurementsCount = dictionary.First().Value.Count();
            int divisor = (measurementsCount - 1) / maxMeasurementsNum + 1;
            var measurementTimes = checkpointsIn.Where((m, i) => i % divisor == 0);

            IList<IEnumerable<Measurement>> list = GetMeasurementsEnumerableList(dictionary, divisor);

            IList<SensorHistoryViewModel> model = list.Select(GetViewModelsFromEnumerable).OrderBy(vm => vm.SensorName).ToList();
            checkpointsOut = measurementTimes;
            return model;
        }

        public IActionResult Download(int? id, string from, string to, int? locationId)
        {
            DateTime fromDate, toDate;
            fromDate = GetFromDateTime(from);
            toDate = GetToDateTime(to);
            MemoryStream resultStream;

            if (locationId != null)
            {
                IEnumerable<DateTime> measurementTimes;
                IDictionary<int, IEnumerable<Measurement>> data = GetData(fromDate, toDate, out measurementTimes, locationId.Value);
                IDictionary<Sensor, IEnumerable<Measurement>> output = new Dictionary<Sensor, IEnumerable<Measurement>>();
                foreach (var kvp in data)
                {
                    output.Add(_context.Sensors.First(s => s.Id == kvp.Key), kvp.Value);
                }

                resultStream = ExcelHelper.Create(output, measurementTimes);
            }
            else if (id == null)
            {
                IEnumerable<DateTime> measurementTimes;
                IDictionary<int, IEnumerable<Measurement>> data = GetData(fromDate, toDate, out measurementTimes);
                IDictionary<Sensor, IEnumerable<Measurement>> output = new Dictionary<Sensor, IEnumerable<Measurement>>();
                foreach (var kvp in data)
                {
                    output.Add(_context.Sensors.First(s => s.Id == kvp.Key), kvp.Value);
                }

                resultStream = ExcelHelper.Create(output, measurementTimes);
            }
            else
            {
                if (_context.Sensors.FirstOrDefault(s => s.Id == id) == null)
                {
                    return NotFound();
                }

                IEnumerable<Measurement> enumerable = GetData(id.Value, fromDate, toDate);
                IDictionary<Sensor, IEnumerable<Measurement>> output = new Dictionary<Sensor, IEnumerable<Measurement>>() { { _context.Sensors.First(s => s.Id == id), enumerable } };
                resultStream = ExcelHelper.Create(output, output.Values.First().Select(m => m.MeasurementTime));
            }

            byte[] array = resultStream.ToArray();
            string fileName = fromDate.ToString("g", CultureInfo.GetCultureInfo("de-DE")) + " - " + toDate.ToString("g", CultureInfo.GetCultureInfo("de-DE")) + (id == null ? "" : $" Id{id.Value}") + ".xlsx";
            return File(array, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private IEnumerable<Measurement> GetData(int id, DateTime fromDate, DateTime toDate)
        {
            return _context.Measurements.AsNoTracking().Where(m => m.SensorId == id && m.MeasurementTime < toDate && m.MeasurementTime > fromDate).OrderBy(m => m.MeasurementTime).AsEnumerable();
        }

        private IDictionary<int, IEnumerable<Measurement>> GetData(DateTime fromDate, DateTime toDate, out IEnumerable<DateTime> measurementTimes, int locationId)
        {
            Dictionary<int, List<Measurement>> dictionary = new Dictionary<int, List<Measurement>>();
            var query = _context.Measurements.Include(m => m.Sensor).AsNoTracking().Where(m => m.Sensor.LocationId == locationId && m.MeasurementTime < toDate && m.MeasurementTime > fromDate);
            var groupedByTime = query.AsEnumerable().GroupBy(m => m.MeasurementTime).OrderBy(g => g.Key);
            var sensorIds = query.Select(m => m.SensorId).Distinct();
            foreach (var sensorId in sensorIds)
            {
                dictionary.Add(sensorId, new List<Measurement>());
            }

            measurementTimes = groupedByTime.Select(g => g.Key);

            foreach (var measurementsInTime in groupedByTime)
            {
                foreach (var keyValuePair in dictionary)
                {
                    if (measurementsInTime.Where(m => m.SensorId == keyValuePair.Key).Count() > 0)
                    {
                        keyValuePair.Value.Add(measurementsInTime.First(m => m.SensorId == keyValuePair.Key));
                    }
                    else
                    {
                        keyValuePair.Value.Add(null);
                    }
                }
            }

            Dictionary<int, IEnumerable<Measurement>> result = new Dictionary<int, IEnumerable<Measurement>>();
            foreach (var keyValuePair in dictionary)
            {
                result.Add(keyValuePair.Key, keyValuePair.Value);
            }
            return result;
        }

        private IDictionary<int, IEnumerable<Measurement>> GetData(DateTime fromDate, DateTime toDate, out IEnumerable<DateTime> measurementTimes)
        {
            Dictionary<int, List<Measurement>> dictionary = new Dictionary<int, List<Measurement>>();
            var query = _context.Measurements.AsNoTracking().Where(m => m.MeasurementTime < toDate && m.MeasurementTime > fromDate);
            var groupedByTime = query.AsEnumerable().GroupBy(m => m.MeasurementTime).OrderBy(g => g.Key);
            var sensorIds = query.Select(m => m.SensorId).Distinct();
            foreach (var sensorId in sensorIds)
            {
                dictionary.Add(sensorId, new List<Measurement>());
            }

            measurementTimes = groupedByTime.Select(g => g.Key);

            foreach (var measurementsInTime in groupedByTime)
            {
                foreach (var keyValuePair in dictionary)
                {
                    if (measurementsInTime.Where(m => m.SensorId == keyValuePair.Key).Count() > 0)
                    {
                        keyValuePair.Value.Add(measurementsInTime.First(m => m.SensorId == keyValuePair.Key));
                    }
                    else
                    {
                        keyValuePair.Value.Add(null);
                    }
                }
            }

            Dictionary<int, IEnumerable<Measurement>> result = new Dictionary<int, IEnumerable<Measurement>>();
            foreach (var keyValuePair in dictionary)
            {
                result.Add(keyValuePair.Key, keyValuePair.Value);
            }
            return result;
        }

        private static int[] GetOffsets(IEnumerable<IGrouping<int, Measurement>> groups, int measurementsCount)
        {
            int[] offsets = new int[groups.Count()];
            int j = 0;
            foreach (var group in groups)
            {
                offsets[j] = measurementsCount - group.Count();
                j++;
            }

            return offsets;
        }

        private static List<IEnumerable<Measurement>> GetMeasurementsEnumerableList(IDictionary<int, IEnumerable<Measurement>> dictionary, int divisor)
        {
            List<IEnumerable<Measurement>> list = new List<IEnumerable<Measurement>>();
            int j = 0;
            foreach (var keyValuePair in dictionary)
            {
                list.Add(keyValuePair.Value.Where((m, i) => i % divisor == 0));
                j++;
            }

            return list;
        }

        private static List<IEnumerable<Measurement>> GetMeasurementsEnumerableList(IEnumerable<IGrouping<int, Measurement>> groups, int[] offsets, int divisor)
        {
            List<IEnumerable<Measurement>> list = new List<IEnumerable<Measurement>>();
            int j = 0;
            foreach (var group in groups)
            {
                int k = j;
                list.Add(group.OrderBy(m => m.MeasurementTime).Where((m, i) => (i + offsets[k]) % divisor == 0));
                j++;
            }

            return list;
        }

        private static DateTime GetFromDateTime(string from)
        {
            DateTime result;
            if (from != null && DateTime.TryParse(from, out result))
            { }
            else
            {
                result = DateTime.Now - TimeSpan.FromDays(1);
            }

            return result;
        }

        private static DateTime GetToDateTime(string to)
        {
            DateTime result;
            if (to != null && DateTime.TryParse(to, out result))
            {
                result += new TimeSpan(23, 59, 59);
            }
            else
            {
                result = DateTime.Now;
            }

            return result;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
