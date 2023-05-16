using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Models.ViewModels;
using TemperatureViewer.Repositories;

namespace TemperatureViewer.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("Admin/{controller}/{action=Index}/{id?}")]
    public class ObserversController : Controller
    {
        //private readonly DefaultContext _context;
        private readonly IObserversRepository _observersRepository;
        private readonly ISensorsRepository _sensorsRepository;

        public ObserversController(IObserversRepository observersRepository, ISensorsRepository sensorsRepository)
        {
            _observersRepository = observersRepository;
            _sensorsRepository = sensorsRepository;
        }

        public async Task<IActionResult> Index()
        {
            return View((await _observersRepository.GetAllAsync()).OrderBy(o => o.Email));//(await _context.Observers.AsNoTracking().OrderBy(o => o.Email).ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            var sensors = (await _sensorsRepository.GetAllAsync()).OrderBy(s => s.Name);//_context.Sensors.AsNoTracking().OrderBy(s => s.Name);
            ViewBag.Sensors = sensors;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ObserverViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sensors = (await _sensorsRepository.GetAllAsync()).OrderBy(s => s.Name);//_context.Sensors.AsNoTracking().OrderBy(s => s.Name);
                return View(viewModel);
            }

            if (viewModel.Sensors == null || viewModel.Sensors.Count == 0)
            {
                ModelState.AddModelError("", "Необходимо выбрать датчики для наблюдения.");
                ViewBag.Sensors = (await _sensorsRepository.GetAllAsync()).OrderBy(s => s.Name); //_context.Sensors.AsNoTracking().OrderBy(s => s.Name);
                return View(viewModel);
            }

            var entity = new Observer() { Email = viewModel.Email,
                Sensors = viewModel.Sensors?.Select(kv => _sensorsRepository.GetByIdAsync(kv.Value).Result).ToList() };//_context.Sensors.FirstOrDefault(s => s.Id == kv.Value)).ToList() };
            await _observersRepository.CreateAsync(entity);
            //_context.Add(entity);
            //await _context.SaveChangesAsync();
            
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Observer observer = await _observersRepository.GetByIdAsync(id.Value, true);//_context.Observers.FirstOrDefaultAsync(o => o.Id == id);
            if (observer == null)
            {
                return NotFound();
            }

            return View(observer);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            Observer observer = await _observersRepository.GetByIdAsync(id.Value, true);//_context.Observers.FindAsync(id);
            if (observer == null)
                return NotFound();

            var sensors = await _sensorsRepository.GetAllAsync();
            ObserverViewModel viewModel = new ObserverViewModel()
            {
                Email = observer.Email,
                SensorsFlags = sensors.ToDictionary(s => s.Id, s => observer.Sensors.Any(os => os.Id == s.Id)) 
            };
            ViewBag.Sensors = sensors.OrderBy(s => s.Name);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ObserverViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (viewModel.Sensors == null || viewModel.Sensors.Count == 0)
            {
                ModelState.AddModelError("", "Необходимо выбрать датчики для наблюдения.");
                Observer observer = await _observersRepository.GetByIdAsync(id, true);//_context.Observers.FindAsync(id);
                if (observer == null)
                    return NotFound();
                var sensors = await _sensorsRepository.GetAllAsync();
                viewModel.SensorsFlags = sensors.ToDictionary(s => s.Id, s => observer.Sensors.Any(os => os.Id == s.Id));
                ViewBag.Sensors = sensors.OrderBy(s => s.Name);
                return View(viewModel);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var entity = await _context.Observers.FindAsync(id);
                    _context.Entry(entity).Collection(o => o.Sensors).Load();
                    entity.Sensors.Clear();
                    _context.Update(entity);
                    await _context.SaveChangesAsync();
                    entity.Sensors = viewModel.Sensors?.Select(kv => _context.Sensors.FirstOrDefault(s => s.Id == kv.Value)).ToList();
                    _context.Update(entity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ObserverExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var observer = await _context.Observers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (observer == null)
            {
                return NotFound();
            }

            return View(observer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var observer = await _context.Observers.FindAsync(id);
            _context.Observers.Remove(observer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ObserverExists(int id)
        {
            return _context.Observers.Any(e => e.Id == id);
        }
    }
}
