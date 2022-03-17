using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TemperatureViewer.BackgroundServices;
using TemperatureViewer.Data;
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

        public IActionResult Index()
        {
            var measurements = _sensorsAccessService.GetMeasurements().OrderBy(e => e.Sensor.Name);
            var viewModel = measurements?.Select(e => new MeasurementViewModel() { Temperature = e.Temperature, SensorName = e.Sensor.Name, SensorId = e.Sensor.Id });
            return View(viewModel);
        }

        public IActionResult History(int? id, string from, string to)       
        {
            DateTime fromDate, toDate;
            fromDate = GetFromDateTime(from);
            toDate = GetToDateTime(to);

            IEnumerable<IGrouping<int, Measurement>> groups;
            if (id != null)
                groups = _context.Measurements.Where(m => m.SensorId == id && m.MeasurementTime < toDate && m.MeasurementTime > fromDate).OrderBy(m => m.MeasurementTime).Include(m => m.Sensor).AsEnumerable().GroupBy(m => m.SensorId);
            else
                groups = _context.Measurements.Where(m => m.MeasurementTime < toDate && m.MeasurementTime > fromDate).OrderBy(m => m.MeasurementTime).Include(m => m.Sensor).AsEnumerable().GroupBy(m => m.SensorId);

            var measurementsCount = groups.Aggregate(0, (c, n) => n.Count() > c ? n.Count() : c);
            int[] offsets = GetOffsets(groups, measurementsCount);

            var maxMeasurementsNum = 50;
            int divisor = (measurementsCount - 1) / maxMeasurementsNum + 1;

            List<IEnumerable<Measurement>> list = GetMeasurementsEnumerableList(groups, offsets, divisor);

            List<SensorHistoryViewModel> model = list.Select(
                g => new SensorHistoryViewModel() { SensorName = g.First().Sensor.Name, Measurements = g.Select(
                    m => new MeasurementOfTime() { Value = m.Temperature, Time = m.MeasurementTime }) 
                }
            ).ToList();
            ViewBag.from = from;
            ViewBag.to = to;
            return View(model);
        }

        public IActionResult Download(int? id, string from, string to)
        {
            DateTime fromDate, toDate;
            fromDate = GetFromDateTime(from);
            toDate = GetToDateTime(to);

            if (id == null)
            {
                Dictionary<int, IEnumerable<Measurement>> data = GetData(fromDate, toDate);

            }
            else
            {
                IEnumerable<Measurement> enumerable = GetData(id.Value, fromDate, toDate);
                Dictionary<Sensor, IEnumerable<Measurement>> data = new Dictionary<Sensor, IEnumerable<Measurement>>() { { _context.Sensors.First(s => s.Id == id), enumerable } };
            }
        }

        private IEnumerable<Measurement> GetData(int id, DateTime fromDate, DateTime toDate)
        {
            return _context.Measurements.Where(m => m.SensorId == id && m.MeasurementTime < toDate && m.MeasurementTime > fromDate).OrderBy(m => m.MeasurementTime).AsEnumerable();
        }

        private Dictionary<int, IEnumerable<Measurement>> GetData(DateTime fromDate, DateTime toDate)
        {
            Dictionary<int, List<Measurement>> lists = new Dictionary<int, List<Measurement>>();
            var groupedByTime = _context.Measurements.Where(m => m.MeasurementTime < toDate && m.MeasurementTime > fromDate).OrderBy(m => m.MeasurementTime).AsEnumerable().GroupBy(m => m.MeasurementTime);
            var sensorIds = _context.Measurements.Where(m => m.MeasurementTime < toDate && m.MeasurementTime > fromDate).Select(m => m.SensorId).Distinct();
            foreach (var sensorId in sensorIds)
            {
                lists.Add(sensorId, null);
            }

            foreach (var measurementsInTime in groupedByTime)
            {
                foreach (var list in lists)
                {
                    if (measurementsInTime.Where(m => m.SensorId == list.Key).Count() > 0)
                    {
                        list.Value.Add(measurementsInTime.First(m => m.SensorId == list.Key));
                    }
                    else
                    {
                        list.Value.Add(null);
                    }
                }
            }

            Dictionary<int, IEnumerable<Measurement>> result = new Dictionary<int, IEnumerable<Measurement>>();
            foreach (var list in lists)
            {
                result.Add(list.Key, list.Value);
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

        private static List<IEnumerable<Measurement>> GetMeasurementsEnumerableList(IEnumerable<IGrouping<int, Measurement>> groups, int[] offsets, int divisor)
        {
            List<IEnumerable<Measurement>> list = new List<IEnumerable<Measurement>>();
            j = 0;
            foreach (var group in groups)
            {
                int k = j;
                list.Add(group.Where((m, i) => (i + offsets[k]) % divisor == 0));
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

        private bool InPeriod(DateTime input, DateTime from, DateTime to)
        {
            if (input < to && input > from)
                return true;
            else
                return false;
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
