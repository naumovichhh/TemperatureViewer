﻿using Microsoft.AspNetCore.Mvc;
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

        public IActionResult History(int? id)
        {
            IEnumerable<IGrouping<int, Measurement>> grouping;
            if (id != null)
                grouping = _context.Measurements.Where(m => m.SensorId == id).Include(m => m.Sensor).AsEnumerable().GroupBy(m => m.SensorId);
            else
                grouping = _context.Measurements.Include(m => m.Sensor).AsEnumerable().GroupBy(m => m.SensorId);

            List<SensorHistoryViewModel> model = grouping.Select(
                g => new SensorHistoryViewModel() { SensorName = g.First().Sensor.Name, Measurements = g.Select(
                    m => new MeasurementOfTime() { Value = m.Temperature, Time = m.MeasurementTime }) 
                }
            ).ToList();
            return View(model);
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
