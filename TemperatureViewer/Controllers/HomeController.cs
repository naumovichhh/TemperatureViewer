using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using TemperatureViewer.BackgroundNAccessServices;
using TemperatureViewer.Database;
using TemperatureViewer.Services;
using TemperatureViewer.Models.DTO;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Models.ViewModels;
using TemperatureViewer.Repositories;

namespace TemperatureViewer.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        //private DefaultContext _context;
        private readonly ISensorsAccessService _sensorsAccessService;
        private readonly ISensorsRepository _sensorsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly InformationService _informationService;

        public HomeController(ILogger<HomeController> logger,
            InformationService informationService,
            DefaultContext context,
            ISensorsAccessService temperatureService,
            ISensorsRepository sensorsRepository,
            ILocationsRepository locationsRepository)
        {
            _logger = logger;
            //_context = context;
            _sensorsAccessService = temperatureService;
            _informationService = informationService;
            _sensorsRepository = sensorsRepository;
            _locationsRepository = locationsRepository;
        }

        public IActionResult Index()
        {
            IEnumerable<ValueDTO> values;
            values = _informationService.GetValues();
            //values = _sensorsAccessService.GetValues();

            var viewModel = values?.Select(e =>
            {
                return new ValueViewModel()
                {
                    Temperature = e.Temperature,
                    SensorName = e.Sensor.Name,
                    SensorId = e.Sensor.Id,
                    Thresholds = e.Thresholds
                };
            });
            ViewBag.location = null;
            return View(viewModel);
        }

        public async Task<IActionResult> HistoryTable(int? id, string from, string to)
        {
            if (id != null)
            {
                if (await _sensorsRepository.GetByIdAsync(id.Value) == null)
                {
                    return NotFound();
                }

                ViewBag.id = id;
            }

            DateTime fromDate, toDate;
            fromDate = GetFromDateTime(from);
            toDate = GetToDateTime(to);
            ViewBag.from = from;
            ViewBag.to = to;
            IEnumerable<DateTime> measurementTimes;
            IList<IEnumerable<Value>> list = _informationService.GetHistoryEnumerableList(id, fromDate, toDate, out measurementTimes);
            if (list == null)
                return View();

            List<SensorHistoryViewModel> model = list.Select(GetViewModelsFromEnumerable).Where(sh => sh != null).OrderBy(vm => vm.SensorName).ToList();
            ViewBag.measurementTimes = measurementTimes.ToList();
            return View(model);
        }

        public async Task<IActionResult> History(int? id, string from, string to)
        {
            if (id != null)
            {
                if ((await _sensorsRepository.GetByIdAsync(id.Value)) == null)
                {
                    return NotFound();
                }

                ViewBag.id = id;
            }
            else
            {
                ViewBag.allSensors = true;
            }

            DateTime fromDate, toDate;
            fromDate = GetFromDateTime(from);
            toDate = GetToDateTime(to);
            ViewBag.from = from;
            ViewBag.to = to;
            ViewBag.allSensors = false;
            IEnumerable<DateTime> measurementTimes;

            
            IList<IEnumerable<Value>> list = _informationService.GetHistoryEnumerableListMax50(id, fromDate, toDate, out measurementTimes);
            if (list == null)
                return View();

            List<SensorHistoryViewModel> model = list.Select(GetViewModelsFromEnumerable).Where(sh => sh != null).OrderBy(vm => vm.SensorName).ToList();
            ViewBag.measurementTimes = measurementTimes;
            return View(model);
        }

        private SensorHistoryViewModel GetViewModelsFromEnumerable(IEnumerable<Value> enumerable)
        {
            if (!enumerable.Any(v => v != null))
                return null;

            return new SensorHistoryViewModel()
            {
                SensorName = _sensorsRepository.GetByIdAsync(enumerable.First(m => m != null).SensorId).Result.Name,//_context.Sensors.AsNoTracking().AsEnumerable().First(s => s.Id == enumerable.First(m => m != null).SensorId).Name,
                Values = enumerable.Select(
                m =>
                {
                    if (m != null)
                        return new ValueOfTime() { Value = m.Temperature, Time = m.MeasurementTime };
                    else
                        return null;
                }).ToList()
            };
        }

        public IActionResult Locations()
        {
            var locationDTOs = _informationService.GetValuesOnLocations();
            var viewModel = locationDTOs.Select(dto => new LocationViewModel() 
            {
                Id = dto.Id, 
                Name = dto.Name, 
                Image = dto.Image,
                Values = dto.Values.Select(v => new ValueViewModel() 
                {
                    Temperature = v.Temperature, 
                    SensorId = v.Sensor.Id, 
                    SensorName = v.Sensor.Name,
                    Thresholds = v.Thresholds
                }) 
            }).ToList();

            return View(viewModel);
        }

        public async Task<IActionResult> ExtendedLocation(int? id, string from, string to)
        {
            if (id == null)
            {
                return NotFound();
            }

            if ((await _locationsRepository.GetByIdAsync(id.Value)) == null)
            {
                return NotFound();
            }

            DateTime fromDate, toDate;
            fromDate = GetFromDateTime(from);
            toDate = GetToDateTime(to);
            ViewBag.from = from;
            ViewBag.to = to;
            IEnumerable<DateTime> checkpoints;

            var locationDTO = _informationService.GetValuesOnLocation(id.Value);


            //Location entity;
            //if ((entity = _context.Locations.AsNoTracking().FirstOrDefault(l => l.Id == id)) == null)
            //{
            //    return NotFound();
            //}

            //DateTime fromDate, toDate;
            //fromDate = GetFromDateTime(from);
            //toDate = GetToDateTime(to);
            //ViewBag.from = from;
            //ViewBag.to = to;
            //IEnumerable<DateTime> checkpoints;

            //var historyDictionary = GetData(fromDate, toDate, out checkpoints, id.Value);
            //IList<SensorHistoryViewModel> history = GetHistoryViewModel(historyDictionary, checkpoints, out checkpoints);

            //var values = _sensorsAccessService.GetValues(entity.Id).Where(e => e != null).OrderBy(e => e.Sensor.Name);
            //var valuesViewModels = values?.Select(e =>
            //{
            //    Threshold threshold = e.Sensor.Threshold ?? new Threshold() { P1 = 12, P2 = 16, P3 = 25, P4 = 30 };
            //    return new ValueViewModel()
            //    {
            //        Temperature = e.Temperature,
            //        SensorName = e.Sensor.Name,
            //        SensorId = e.Sensor.Id,
            //        Thresholds = new int[] { threshold.P1, threshold.P2, threshold.P3, threshold.P4 }
            //    };
            //});

            //ExtendedLocationViewModel viewModel = new ExtendedLocationViewModel()
            //{
            //    Name = entity.Name,
            //    Image = entity.Image,
            //    History = history,
            //    HistoryCheckpoints = checkpoints,
            //    Values = valuesViewModels
            //};

            //ViewBag.locationId = id;
            //return View(viewModel);
        }

        private IList<SensorHistoryViewModel> GetHistoryViewModel(IDictionary<int, IEnumerable<Value>> dictionary, IEnumerable<DateTime> checkpointsIn, out IEnumerable<DateTime> checkpointsOut)
        {
            if (!dictionary.All(e => e.Value.Count() == dictionary.First().Value.Count()))
                throw new ArgumentException("Data must contain enumerables of values of equal length.", nameof(dictionary));

            if (dictionary.Count() == 0)
            {
                checkpointsOut = null;
                return null;
            }

            var maxValuesNum = 50;
            var valuesCount = dictionary.First().Value.Count();
            int divisor = (valuesCount - 1) / maxValuesNum + 1;
            var measurementTimes = checkpointsIn.Where((m, i) => i % divisor == 0);

            IList<IEnumerable<Value>> list = GetValuesEnumerableList(dictionary, divisor);

            IList<SensorHistoryViewModel> model = list.Select(GetViewModelsFromEnumerable).Where(vm => vm != null).OrderBy(vm => vm.SensorName).ToList();
            checkpointsOut = measurementTimes;
            return model;
        }

        public IActionResult Download(int? id, string from, string to, int? locationId)
        {
            DateTime fromDate, toDate;
            fromDate = GetFromDateTime(from);
            toDate = GetToDateTime(to);
            MemoryStream resultStream;

            if (locationId != null)
            {
                IEnumerable<DateTime> measurementTimes;
                IDictionary<int, IEnumerable<Value>> data = GetData(fromDate, toDate, out measurementTimes, locationId.Value);
                IDictionary<Sensor, IEnumerable<Value>> output = new Dictionary<Sensor, IEnumerable<Value>>();
                foreach (var kvp in data)
                {
                    output.Add(_context.Sensors.First(s => s.Id == kvp.Key), kvp.Value);
                }

                resultStream = ExcelService.Create(output, measurementTimes);
            }
            else if (id == null)
            {
                IEnumerable<DateTime> measurementTimes;
                IDictionary<int, IEnumerable<Value>> data = GetData(fromDate, toDate, out measurementTimes);
                IDictionary<Sensor, IEnumerable<Value>> output = new Dictionary<Sensor, IEnumerable<Value>>();
                foreach (var kvp in data)
                {
                    output.Add(_context.Sensors.First(s => s.Id == kvp.Key), kvp.Value);
                }

                resultStream = ExcelService.Create(output, measurementTimes);
            }
            else
            {
                if (_context.Sensors.FirstOrDefault(s => s.Id == id) == null)
                {
                    return NotFound();
                }

                IEnumerable<Value> enumerable = GetData(id.Value, fromDate, toDate);
                IDictionary<Sensor, IEnumerable<Value>> output = new Dictionary<Sensor, IEnumerable<Value>>() { { _context.Sensors.First(s => s.Id == id), enumerable } };
                resultStream = ExcelService.Create(output, output.Values.First().Select(m => m.MeasurementTime));
            }

            byte[] array = resultStream.ToArray();
            string fileName = fromDate.ToString("g", CultureInfo.GetCultureInfo("de-DE")) + " - " + toDate.ToString("g", CultureInfo.GetCultureInfo("de-DE")) + (id == null ? "" : $" Id{id.Value}") + ".xlsx";
            return File(array, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        

        private IDictionary<int, IEnumerable<Value>> GetData(DateTime fromDate, DateTime toDate, out IEnumerable<DateTime> measurementTimes, int locationId)
        {
            Dictionary<int, List<Value>> dictionary = new Dictionary<int, List<Value>>();
            var query = _context.Values.Include(m => m.Sensor).AsNoTracking().Where(m => m.Sensor.LocationId == locationId && m.MeasurementTime < toDate && m.MeasurementTime > fromDate);
            var groupedByTime = query.AsEnumerable().GroupBy(m => m.MeasurementTime).OrderBy(g => g.Key);
            var sensorIds = query.Select(m => m.SensorId).Distinct();
            foreach (var sensorId in sensorIds)
            {
                dictionary.Add(sensorId, new List<Value>());
            }

            measurementTimes = groupedByTime.Select(g => g.Key);

            foreach (var valuesInTime in groupedByTime)
            {
                foreach (var keyValuePair in dictionary)
                {
                    if (valuesInTime.Where(m => m.SensorId == keyValuePair.Key).Count() > 0)
                    {
                        keyValuePair.Value.Add(valuesInTime.First(m => m.SensorId == keyValuePair.Key));
                    }
                    else
                    {
                        keyValuePair.Value.Add(null);
                    }
                }
            }

            Dictionary<int, IEnumerable<Value>> result = new Dictionary<int, IEnumerable<Value>>();
            foreach (var keyValuePair in dictionary)
            {
                result.Add(keyValuePair.Key, keyValuePair.Value);
            }
            return result;
        }

        private static int[] GetOffsets(IEnumerable<IGrouping<int, Value>> groups, int valuesCount)
        {
            int[] offsets = new int[groups.Count()];
            int j = 0;
            foreach (var group in groups)
            {
                offsets[j] = valuesCount - group.Count();
                j++;
            }

            return offsets;
        }

        private static DateTime GetFromDateTime(string from)
        {
            DateTime result;
            if (from != null && DateTime.TryParse(from, out result))
            { }
            else
            {
                result = DateTime.Now - TimeSpan.FromDays(1);
            }

            return result;
        }

        private static DateTime GetToDateTime(string to)
        {
            DateTime result;
            if (to != null && DateTime.TryParse(to, out result))
            {
                result += new TimeSpan(23, 59, 59);
            }
            else
            {
                result = DateTime.Now;
            }

            return result;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
