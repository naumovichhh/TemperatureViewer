using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Repositories;

namespace TemperatureViewer.Components
{
    public class LocationsDropdown : ViewComponent
    {
        private ILocationsRepository _repository;

        public LocationsDropdown(ILocationsRepository repository)
        {
            _repository = repository;
        }

        public IViewComponentResult Invoke()
        {
            var locations = _repository.GetAllAsync().Result.OrderBy(l => l.Name);
            return View(locations);
        }
    }
}
