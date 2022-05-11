using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Data;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Models.ViewModels;

namespace TemperatureViewer.Controllers
{
    [Authorize]
    [Route("Admin/{controller}/{action=Index}/{id?}")]
    public class ObserversController : Controller
    {
        private readonly DefaultContext _context;

        public ObserversController(DefaultContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Observers.AsNoTracking().OrderBy(o => o.Email).ToListAsync());
        }

        public IActionResult Create()
        {
            var sensors = _context.Sensors.AsNoTracking().OrderBy(s => s.Name);
            ViewBag.Sensors = sensors;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ObserverViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sensors = _context.Sensors;
                return View(viewModel);
            }

            if (viewModel.Sensors == null || viewModel.Sensors.Count == 0)
            {
                ModelState.AddModelError("", "Необходимо выбрать датчики для наблюдения.");
                ViewBag.Sensors = _context.Sensors;
                return View(viewModel);
            }

            var entity = new Observer() { Email = viewModel.Email, Sensors = viewModel.Sensors.Select(kv => _context.Sensors.FirstOrDefault(s => s.Id == kv.Value)).ToList() };
            _context.Add(entity);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var observer = await _context.Observers
                .FirstOrDefaultAsync(m => m.Email == id);
            if (observer == null)
            {
                return NotFound();
            }

            return View(observer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var observer = await _context.Observers.FindAsync(id);
            _context.Observers.Remove(observer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ObserverExists(string id)
        {
            return _context.Observers.Any(e => e.Email == id);
        }
    }
}
