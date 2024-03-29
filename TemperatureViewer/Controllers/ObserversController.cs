﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Models.ViewModels;
using TemperatureViewer.Repositories;

namespace TemperatureViewer.Controllers
{
    [Authorize(Roles = "admin,operator")]
    [Route("Admin/{controller}/{action=Index}/{id?}")]
    public class ObserversController : Controller
    {
        private readonly IObserversRepository _observersRepository;
        private readonly ISensorsRepository _sensorsRepository;

        public ObserversController(IObserversRepository observersRepository, ISensorsRepository sensorsRepository)
        {
            _observersRepository = observersRepository;
            _sensorsRepository = sensorsRepository;
        }

        public async Task<IActionResult> Index()
        {
            return View((await _observersRepository.GetAllAsync()).OrderBy(o => o.Email));
        }

        public async Task<IActionResult> Create()
        {
            var sensors = (await _sensorsRepository.GetAllAsync()).OrderBy(s => s.Name);
            ViewBag.Sensors = sensors;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ObserverViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sensors = (await _sensorsRepository.GetAllAsync()).OrderBy(s => s.Name);
                return View(viewModel);
            }

            if (viewModel.Sensors == null || viewModel.Sensors.Count == 0)
            {
                ModelState.AddModelError("", "Необходимо выбрать датчики для наблюдения.");
                ViewBag.Sensors = (await _sensorsRepository.GetAllAsync()).OrderBy(s => s.Name);
                return View(viewModel);
            }

            var entity = new Observer() { Email = viewModel.Email,
                Sensors = viewModel.Sensors?.Select(kv => _sensorsRepository.GetByIdAsync(kv.Value).Result).ToList() };
            await _observersRepository.CreateAsync(entity);
            
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Observer observer = await _observersRepository.GetByIdAsync(id.Value, true);
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

            Observer observer = await _observersRepository.GetByIdAsync(id.Value, true);
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
                Observer observer = await _observersRepository.GetByIdAsync(id, true);
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
                    var entity = await _observersRepository.GetByIdAsync(id, true);
                    entity.Email = viewModel.Email;
                    entity.Sensors.Clear();
                    await _observersRepository.UpdateAsync(entity);
                    entity.Sensors = viewModel.Sensors?.Select(kv => _sensorsRepository.GetByIdAsync(kv.Value).Result).ToList();
                    await _observersRepository.UpdateAsync(entity);
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

            var observer = await _observersRepository.GetByIdAsync(id.Value);
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
            await _observersRepository.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        private bool ObserverExists(int id)
        {
            return _observersRepository.GetByIdAsync(id).Result != null;
        }
    }
}
