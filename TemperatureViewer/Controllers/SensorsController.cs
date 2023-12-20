using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Repositories;

namespace TemperatureViewer.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("Admin/{controller}/{action=Index}/{id?}")]
    public class SensorsController : Controller
    {
        private readonly ISensorsRepository _sensorsRepository;
        private readonly ILocationsRepository _locationsRepository;

        public SensorsController(ISensorsRepository sensorsRepository, ILocationsRepository locationsRepository)
        {
            _sensorsRepository = sensorsRepository;
            _locationsRepository = locationsRepository;
        }

        // GET: Sensors
        public async Task<IActionResult> Index()
        {
            return View((await _sensorsRepository.GetAllAsync()).OrderBy(s => s.Name).ToList());
        }

        // GET: Sensors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sensor = await _sensorsRepository.GetByIdAsync(id.Value);
            if (sensor == null)
            {
                return NotFound();
            }

            return View(sensor);
        }

        // GET: Sensors/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Locations = await _locationsRepository.GetAllAsync();
            return View();
        }

        // POST: Sensors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Sensor sensor)
        {
            if (ModelState.IsValid)
            {
                await _sensorsRepository.CreateAsync(sensor);
                return RedirectToAction(nameof(Index));
            }
            return View(sensor);
        }

        // GET: Sensors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            //if (id == null)
            //{
            //    return NotFound();
            //}

            var sensor = await _sensorsRepository.GetByIdAsync(id.Value);
            if (sensor == null)
            {
                return NotFound();
            }

            ViewBag.Locations = await _locationsRepository.GetAllAsync();
            return View(sensor);
        }

        // POST: Sensors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Sensor sensor)
        {
            if (id != sensor.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Sensor fromRepository = await _sensorsRepository.GetByIdAsync(sensor.Id);
                    fromRepository.Name = sensor.Name;
                    fromRepository.Uri = sensor.Uri;
                    fromRepository.LocationId = sensor.LocationId;
                    fromRepository.XPath = sensor.XPath;
                    fromRepository.Regex = sensor.Regex;
                    await _sensorsRepository.UpdateAsync(fromRepository);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SensorExists(sensor.Id))
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
            return View(sensor);
        }

        // GET: Sensors/Delete/5
        public async Task<IActionResult> Disable(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sensor = await _sensorsRepository.GetByIdAsync(id.Value);
            if (sensor == null)
            {
                return NotFound();
            }

            return View(sensor);
        }

        // POST: Sensors/Delete/5
        [HttpPost, ActionName("Disable")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DisableConfirmed(int id)
        {
            var sensor = await _sensorsRepository.GetByIdAsync(id);
            sensor.WasDisabled = true;
            await _sensorsRepository.UpdateAsync(sensor);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Enable(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sensor = await _sensorsRepository.GetByIdAsync(id.Value);
            if (sensor == null)
            {
                return NotFound();
            }

            return View(sensor);
        }

        [HttpPost, ActionName("Enable")]
        public async Task<IActionResult> EnableConfirmed(int id)
        {
            var sensor = await _sensorsRepository.GetByIdAsync(id);
            sensor.WasDisabled = false;
            await _sensorsRepository.UpdateAsync(sensor);
            return RedirectToAction(nameof(Index));
        }

        private bool SensorExists(int id)
        {
            return _sensorsRepository.GetByIdAsync(id) != null;
        }
    }
}
