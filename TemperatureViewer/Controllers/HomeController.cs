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
            var viewModel = measurements.Select(e => new MeasurementViewModel() { Temperature = e.Temperature, SensorName = e.Sensor.Name });
            return View(viewModel);
        }

        public IActionResult History(int? id)
        {
            return View();
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
