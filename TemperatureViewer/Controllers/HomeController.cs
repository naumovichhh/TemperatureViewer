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
            var measurements = _sensorsAccessService.GetMeasurements();
            var viewModel = measurements?.Select(e => new MeasurementViewModel() { Temperature = e.Temperature, SensorName = e.Sensor.Name, SensorId = e.Sensor.Id });
            return View(viewModel);
        }

        public IActionResult History(int? id, string from, string to)       
        {
            DateTime fromDate, toDate;
            if (from != null && DateTime.TryParse(from, out fromDate))
                ;
            else
            {
                fromDate = DateTime.Now - TimeSpan.FromDays(1);
            }

            if (to != null && DateTime.TryParse(to, out toDate))
                ;
            else
            {
                toDate = DateTime.Now;
            }

            IEnumerable<IGrouping<int, Measurement>> groups;
            if (id != null)
                groups = _context.Measurements.Where(m => m.SensorId == id && m.MeasurementTime < toDate && m.MeasurementTime > fromDate).OrderBy(m => m.MeasurementTime).Include(m => m.Sensor).AsEnumerable().GroupBy(m => m.SensorId);
            else
                groups = _context.Measurements.Where(m => m.MeasurementTime < toDate && m.MeasurementTime > fromDate).OrderBy(m => m.MeasurementTime).Include(m => m.Sensor).AsEnumerable().GroupBy(m => m.SensorId);

            var measurementsCount = groups.Aggregate(0, (c, n) => n.Count() > c ? n.Count() : c);
            int[] offsets = new int[groups.Count()];
            int j = 0;
            foreach (var group in groups)
            {
                offsets[j] = measurementsCount - group.Count();
                j++;
            }

            var maxMeasurementsNum = 50;
            int divisor = (measurementsCount - 1) / maxMeasurementsNum + 1;

            List<IEnumerable<Measurement>> list = new List<IEnumerable<Measurement>>();
            j = 0;
            foreach (var group in groups)
            {
                int k = j;
                list.Add(group.Where((m, i) => (i + offsets[k]) % divisor == 0));
                j++;
            }

            List<SensorHistoryViewModel> model = list.Select(
                g => new SensorHistoryViewModel() { SensorName = g.First().Sensor.Name, Measurements = g.Select(
                    m => new MeasurementOfTime() { Value = m.Temperature, Time = m.MeasurementTime }) 
                }
            ).ToList();
            return View(model);
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
