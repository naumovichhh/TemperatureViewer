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
using TemperatureViewer.Models;

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

        public IActionResult Index(int locationId)
        {
            IEnumerable<Measurement> measurements;
            if (locationId != 0)
            {
                measurements = _sensorsAccessService.GetMeasurements(locationId).OrderBy(e => e.Sensor.Name);
            }
            else
            {
                measurements = _sensorsAccessService.GetMeasurements().OrderBy(e => e.Sensor.Name);
            }

            var viewModel = measurements?.Select(e => new MeasurementViewModel() { Temperature = e.Temperature, SensorName = e.Sensor.Name, SensorId = e.Sensor.Id });
            return View(viewModel);
        }

        public IActionResult History(int? id, string from, string to)
        {
            DateTime fromDate, toDate;
            fromDate = GetFromDateTime(from);
            toDate = GetToDateTime(to);
            IEnumerable<DateTime> measurementTimes;

            Dictionary<int, IEnumerable<Measurement>> dictionary;
            if (id != null)
            {
                if (_context.Sensors.FirstOrDefault(s => s.Id == id) == null)
                {
                    return NotFound();
                }

                IEnumerable<Measurement> enumerable = GetData(id.Value, fromDate, toDate);
                measurementTimes = enumerable.Select(m => m.MeasurementTime);
                dictionary = new Dictionary<int, IEnumerable<Measurement>>() { { id.Value, enumerable } };
            }
            else
            {
                dictionary = GetData(fromDate, toDate, out measurementTimes);
            }

            if (!dictionary.All(e => e.Value.Count() == dictionary.First().Value.Count()))
                throw new ArgumentException("Data must contain enumerables of measurements of equal length.", nameof(dictionary));

            var maxMeasurementsNum = 50;
            var measurementsCount = dictionary.First().Value.Count();
            int divisor = (measurementsCount - 1) / maxMeasurementsNum + 1;
            measurementTimes = measurementTimes.Where((m, i) => i % divisor == 0);

            List<IEnumerable<Measurement>> list = GetMeasurementsEnumerableList(dictionary, divisor);

            List<SensorHistoryViewModel> model = list.Select(
                g => new SensorHistoryViewModel()
                {
                    SensorName = _context.Sensors.AsNoTracking().AsEnumerable().First(s => s.Id == g.First(m => m != null).SensorId).Name,
                    Measurements = g.Select(
                    m =>
                    {
                        if (m != null)
                            return new MeasurementOfTime() { Value = m.Temperature, Time = m.MeasurementTime };
                        else
                            return null;
                    })
                }
            ).OrderBy(vm => vm.SensorName).ToList();
            ViewBag.measurementTimes = measurementTimes;
            ViewBag.from = from;
            ViewBag.to = to;
            return View(model);
        }

        public IActionResult Locations()
        {
            var locations = _context.Location.AsNoTracking().AsEnumerable();
            return View(locations);
        }

        //public IActionResult History(int? id, string from, string to, string loh)       
        //{
        //    DateTime fromDate, toDate;
        //    fromDate = GetFromDateTime(from);
        //    toDate = GetToDateTime(to);

        //    IEnumerable<IGrouping<int, Measurement>> groups;
        //    if (id != null)
        //    {
        //        if (_context.Sensors.FirstOrDefault(s => s.Id == id) == null)
        //        {
        //            return NotFound();
        //        }

        //        groups = _context.Measurements.Where(m => m.SensorId == id && m.MeasurementTime < toDate && m.MeasurementTime > fromDate).OrderBy(m => m.MeasurementTime).Include(m => m.Sensor).AsEnumerable().GroupBy(m => m.SensorId);
        //    }
        //    else
        //        groups = _context.Measurements.Where(m => m.MeasurementTime < toDate && m.MeasurementTime > fromDate).OrderBy(m => m.MeasurementTime).Include(m => m.Sensor).AsEnumerable().GroupBy(m => m.SensorId);

        //    var measurementsCount = groups.Aggregate(0, (c, n) => n.Count() > c ? n.Count() : c);
        //    int[] offsets = GetOffsets(groups, measurementsCount);

        //    var maxMeasurementsNum = 50;
        //    int divisor = (measurementsCount - 1) / maxMeasurementsNum + 1;

        //    List<IEnumerable<Measurement>> list = GetMeasurementsEnumerableList(groups, offsets, divisor);

        //    List<SensorHistoryViewModel> model = list.Select(
        //        g => new SensorHistoryViewModel() { SensorName = g.First().Sensor.Name, Measurements = g.Select(
        //            m => new MeasurementOfTime() { Value = m.Temperature, Time = m.MeasurementTime }) 
        //        }
        //    ).ToList();
        //    ViewBag.from = from;
        //    ViewBag.to = to;
        //    return View(model);
        //}

        public IActionResult Download(int? id, string from, string to)
        {
            DateTime fromDate, toDate;
            fromDate = GetFromDateTime(from);
            toDate = GetToDateTime(to);
            MemoryStream resultStream;

            if (id == null)
            {
                IEnumerable<DateTime> useless;
                Dictionary<int, IEnumerable<Measurement>> data = GetData(fromDate, toDate, out useless);
                Dictionary<Sensor, IEnumerable<Measurement>> output = new Dictionary<Sensor, IEnumerable<Measurement>>();
                foreach (var kvp in data)
                {
                    output.Add(_context.Sensors.First(s => s.Id == kvp.Key), kvp.Value);
                }

                resultStream = ExcelHelper.Create(output);
            }
            else
            {
                if (_context.Sensors.FirstOrDefault(s => s.Id == id) == null)
                {
                    return NotFound();
                }

                IEnumerable<Measurement> enumerable = GetData(id.Value, fromDate, toDate);
                Dictionary<Sensor, IEnumerable<Measurement>> output = new Dictionary<Sensor, IEnumerable<Measurement>>() { { _context.Sensors.First(s => s.Id == id), enumerable } };
                resultStream = ExcelHelper.Create(output);
            }

            byte[] array = resultStream.ToArray();
            string fileName = fromDate.ToString("g", CultureInfo.GetCultureInfo("de-DE")) + " - " + toDate.ToString("g", CultureInfo.GetCultureInfo("de-DE")) + (id == null ? "" : $" Id{id.Value}") + ".xlsx";
            return File(array, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        private IEnumerable<Measurement> GetData(int id, DateTime fromDate, DateTime toDate)
        {
            return _context.Measurements.AsNoTracking().Where(m => m.SensorId == id && m.MeasurementTime < toDate && m.MeasurementTime > fromDate).OrderBy(m => m.MeasurementTime).AsEnumerable();
        }

        private Dictionary<int, IEnumerable<Measurement>> GetData(DateTime fromDate, DateTime toDate, out IEnumerable<DateTime> measurementTimes)
        {
            Dictionary<int, List<Measurement>> dictionary = new Dictionary<int, List<Measurement>>();
            var groupedByTime = _context.Measurements.AsNoTracking().Where(m => m.MeasurementTime < toDate && m.MeasurementTime > fromDate).AsEnumerable().GroupBy(m => m.MeasurementTime).OrderBy(g => g.Key);
            var sensorIds = _context.Measurements.AsNoTracking().Where(m => m.MeasurementTime < toDate && m.MeasurementTime > fromDate).Select(m => m.SensorId).Distinct();
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

        private static List<IEnumerable<Measurement>> GetMeasurementsEnumerableList(Dictionary<int, IEnumerable<Measurement>> dictionary, int divisor)
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
        
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> About()
        {
            IQueryable<MeasurementDateGroup> data =
                from measurement in _context.Measurements
                group measurement by measurement.MeasurementTime.Date into dateGroup
                select new MeasurementDateGroup()
                {
                    MeasurementsCount = dateGroup.Count(),
                    MeasurementDate = dateGroup.Key
                };
            return View(await data.AsNoTracking().ToListAsync());
        }
    }
}
