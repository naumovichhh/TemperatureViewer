using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Data;
using TemperatureViewer.Models;

namespace TemperatureViewer
{
    [Route("Admin/{Controller}/{action=Index}")]
    [Authorize]
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
        public async Task<IActionResult> Create(ThresholdViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sensors = _context.Sensors;
                return View(viewModel);
            }

            if (viewModel.P1 > viewModel.P2 || viewModel.P2 > viewModel.P3 || viewModel.P3 > viewModel.P4)
            {
                ModelState.AddModelError("", "Значения более высоких порогов не должны быть меньше.");
                ViewBag.Sensors = _context.Sensors;
                return View(viewModel);
            }

            if (viewModel.Sensors == null || viewModel.Sensors.Count == 0)
            {
                ModelState.AddModelError("", "Необходимо выбрать датчики для применения порогов.");
                ViewBag.Sensors = _context.Sensors;
                return View(viewModel);
            }


            Threshold entity = new Threshold() { P1 = viewModel.P1, P2 = viewModel.P2, P3 = viewModel.P3, P4 = viewModel.P4 };
            _context.Thresholds.Add(entity);
            await _context.SaveChangesAsync();
            foreach (var sensor in viewModel.Sensors)
            {
                int id = sensor.Value;
                _context.Sensors.FirstOrDefault(s => s.Id == id).ThresholdId = entity.Id;
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
    }
}
