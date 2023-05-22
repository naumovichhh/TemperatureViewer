using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Models.ViewModels;
using TemperatureViewer.Repositories;

namespace TemperatureViewer
{
    [Route("Admin/{Controller}/{action=Index}")]
    [Authorize(Roles = "admin,operator")]
    public class ThresholdsController : Controller
    {
        //private DefaultContext _context;
        private readonly IThresholdsRepository _thresholdsRepository;
        private readonly ISensorsRepository _sensorsRepository;

        public ThresholdsController(IThresholdsRepository thresholdsRepository, ISensorsRepository sensorsRepository)
        {
            _thresholdsRepository = thresholdsRepository;
            _sensorsRepository = sensorsRepository;
        }

        public async Task<IActionResult> Index()
        {
            var entities = await _thresholdsRepository.GetAllAsync(true);//_context.Thresholds.AsNoTracking().Include(t => t.Sensors);
            var viewModels = entities.Select(e => new ThresholdViewModel() { P1 = e.P1, P2 = e.P2, P3 = e.P3, P4 = e.P4, SensorNames = e.Sensors.OrderBy(s => s.Name).Select(s => s.Name).ToList() });
            return View(viewModels);
        }

        public async Task<IActionResult> Create()
        {
            var sensors = (await _sensorsRepository.GetAllAsync()).OrderBy(s => s.Name);//_context.Sensors.AsNoTracking().OrderBy(s => s.Name);
            ViewBag.Sensors = sensors;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThresholdViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Sensors = (await _sensorsRepository.GetAllAsync()).OrderBy(s => s.Name);//_context.Sensors;
                return View(viewModel);
            }

            if (viewModel.P1 > viewModel.P2 || viewModel.P2 > viewModel.P3 || viewModel.P3 > viewModel.P4)
            {
                ModelState.AddModelError("", "Значения более высоких порогов не должны быть меньше.");
                ViewBag.Sensors = (await _sensorsRepository.GetAllAsync()).OrderBy(s => s.Name);//_context.Sensors;
                return View(viewModel);
            }

            if (viewModel.Sensors == null || viewModel.Sensors.Count == 0)
            {
                ModelState.AddModelError("", "Необходимо выбрать датчики для применения порогов.");
                ViewBag.Sensors = (await _sensorsRepository.GetAllAsync()).OrderBy(s => s.Name);//_context.Sensors;
                return View(viewModel);
            }


            Threshold entity;
            if ((entity = GetExistingThreshold(viewModel)) == null)
            { 
                entity = new Threshold() { P1 = viewModel.P1, P2 = viewModel.P2, P3 = viewModel.P3, P4 = viewModel.P4 };
                await _thresholdsRepository.CreateAsync(entity);
                //_context.Thresholds.Add(entity);
                //await _context.SaveChangesAsync();
            }
                
            foreach (var sensor in viewModel.Sensors)
            {
                int id = sensor.Value;
                var sensorEntity = await _sensorsRepository.GetByIdAsync(id);
                sensorEntity.ThresholdId = entity.Id;
                await _sensorsRepository.UpdateAsync(sensorEntity);
            }

            var uselessThresholds = (await _thresholdsRepository.GetAllAsync(true)).Where(t => !t.Sensors.Any());//_context.Thresholds.Include(t => t.Sensors).Where(t => !t.Sensors.Any());
            foreach (var uselessThreshold in uselessThresholds)
            {
                await _thresholdsRepository.DeleteAsync(uselessThreshold.Id);
            }

            return RedirectToAction("Index", "Home");
        }

        private Threshold GetExistingThreshold(ThresholdViewModel viewModel)
        {
            return (_thresholdsRepository.GetAllAsync().Result).FirstOrDefault(t => t.P1 == viewModel.P1 && t.P2 == viewModel.P2 && t.P3 == viewModel.P3 && t.P4 == viewModel.P4);//_context.Thresholds.AsNoTracking().FirstOrDefault(t => t.P1 == viewModel.P1 && t.P2 == viewModel.P2 && t.P3 == viewModel.P3 && t.P4 == viewModel.P4);
        }
    }
}
