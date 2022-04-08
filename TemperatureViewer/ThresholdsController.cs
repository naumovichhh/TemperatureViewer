using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Data;
using TemperatureViewer.Models;

namespace TemperatureViewer
{
    [Route("Admin/{Controller}")]
    public class ThresholdsController : Controller
    {
        private DefaultContext _context;

        public ThresholdsController(DefaultContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            var sensors = _context.Sensors;
            ViewBag.Sensors = sensors;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Threshold threshold)
        {
            if (ModelState.IsValid)
            {
                _context.Thresholds.Add(threshold);
                await _context.SaveChangesAsync();
                foreach (var sensor in threshold.Sensors)
                {
                    int id = sensor.Id;
                    _context.Sensors.FirstOrDefault(s => s.Id == id).ThresholdId = threshold.Id;
                }
                await _context.SaveChangesAsync();

                var uselessThresholds = _context.Thresholds.Include(t => t.Sensors).Where(t => !t.Sensors.Any());
                foreach (var uselessThreshold in uselessThresholds)
                {
                    _context.Entry(uselessThreshold).State = EntityState.Deleted;
                }
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Sensors = _context.Sensors;
                return View(threshold);
            }
        }
    }
}
