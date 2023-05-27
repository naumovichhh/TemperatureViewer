using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TemperatureViewer.BackgroundNAccessServices;
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
        private readonly ISensorsAccessService _sensorsAccessService;
        private readonly ISensorsRepository _sensorsRepository;
        private readonly ILocationsRepository _locationsRepository;
        private readonly InformationService _informationService;

        public HomeController(InformationService informationService,
            ISensorsAccessService temperatureService,
            ISensorsRepository sensorsRepository,
            ILocationsRepository locationsRepository)
        {
            _sensorsAccessService = temperatureService;
            _informationService = informationService;
            _sensorsRepository = sensorsRepository;
            _locationsRepository = locationsRepository;
        }

        public IActionResult Index()
        {
            IEnumerable<ValueDTO> values;
            if (User.IsInRole("user"))
            {
                string userName = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value;
                values = _informationService.GetValues(userName);
            }
            else
            {
                values = _informationService.GetValues();
            }

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
            IList<IEnumerable<Value>> list;
            if (User.IsInRole("user"))
            {
                string userName = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value;
                list = _informationService.GetHistoryEnumerableList(id, fromDate, toDate, out measurementTimes, userName);
            }
            else
            {
                list = _informationService.GetHistoryEnumerableList(id, fromDate, toDate, out measurementTimes);
            }
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


            IList<IEnumerable<Value>> list;
            if (User.IsInRole("user"))
            {
                string userName = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value;
                list = _informationService.GetHistoryEnumerableListMax50(id, fromDate, toDate, out measurementTimes, userName);
            }
            else
            {
                list = _informationService.GetHistoryEnumerableListMax50(id, fromDate, toDate, out measurementTimes);
            }

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
            IList<LocationDTO> locationDTOs;
            if (User.IsInRole("user"))
            {
                string userName = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value;
                locationDTOs = _informationService.GetValuesOnLocations(userName);
            }
            else
            {
                locationDTOs = _informationService.GetValuesOnLocations();
            }
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
            LocationDTO locationDTO;
            IList<IEnumerable<Value>> historyEnumerableList;
            if (User.IsInRole("user"))
            {
                string userName = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value;
                locationDTO = _informationService.GetValuesOnLocation(id.Value, userName);
                historyEnumerableList = _informationService.GetLocationHistoryEnumerableListMax50(
                    id.Value,
                    fromDate,
                    toDate,
                    out checkpoints,
                    userName);
            }
            else
            {
                locationDTO = _informationService.GetValuesOnLocation(id.Value);
                historyEnumerableList = _informationService.GetLocationHistoryEnumerableListMax50(
                    id.Value,
                    fromDate,
                    toDate,
                    out checkpoints);
            }

            IList<SensorHistoryViewModel> historyViewModel = historyEnumerableList
                ?.Select(GetViewModelsFromEnumerable)
                .Where(vm => vm != null)
                .OrderBy(vm => vm.SensorName)
                .ToList();
            ExtendedLocationViewModel viewModel = new ExtendedLocationViewModel()
            {
                Name = locationDTO.Name,
                Image = locationDTO.Image,
                History = historyViewModel,
                HistoryCheckpoints = checkpoints,
                Values = locationDTO.Values.Select(dto => new ValueViewModel()
                { 
                    SensorId = dto.Sensor.Id,
                    SensorName = dto.Sensor.Name,
                    Temperature = dto.Temperature,
                    Thresholds = dto.Thresholds
                })
            };

            ViewBag.locationId = id;
            return View(viewModel);
        }

        public async Task<IActionResult> Download(int? id, string from, string to, int? locationId)
        {
            DateTime fromDate, toDate;
            fromDate = GetFromDateTime(from);
            toDate = GetToDateTime(to);
            if (locationId != null)
            {
                if (await _locationsRepository.GetByIdAsync(locationId.Value) == null)
                    return NotFound();
            }
            else if (id != null)
            {
                if (await _sensorsRepository.GetByIdAsync(id.Value) == null)
                    return NotFound();
            }

            byte[] fileArray;
            if (User.IsInRole("user"))
            {
                string userName = User.Claims.FirstOrDefault(c => c.Type == ClaimsIdentity.DefaultNameClaimType)?.Value;
                fileArray = _informationService.DownloadExcel(fromDate, toDate, id, locationId, userName);
            }
            else
            {
                fileArray = _informationService.DownloadExcel(fromDate, toDate, id, locationId);
            }
            string fileName = fromDate.ToString("g", CultureInfo.GetCultureInfo("de-DE")) + " - " + toDate.ToString("g", CultureInfo.GetCultureInfo("de-DE")) + (id == null ? "" : $" Id{id.Value}") + ".xlsx";
            return File(fileArray, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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
