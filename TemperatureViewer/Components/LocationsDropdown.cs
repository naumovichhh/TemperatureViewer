using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Database;

namespace TemperatureViewer.Components
{
    public class LocationsDropdown : ViewComponent
    {
        private DefaultContext _context;

        public LocationsDropdown(DefaultContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var locations = _context.Locations.AsNoTracking().OrderBy(l => l.Name).AsEnumerable();
            return View(locations);
        }
    }
}
