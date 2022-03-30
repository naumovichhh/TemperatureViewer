using Microsoft.AspNetCore.Mvc;

namespace TemperatureViewer.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
