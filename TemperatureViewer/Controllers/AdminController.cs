using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TemperatureViewer.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Thresholds()
        {
            if (System.IO.File.Exists("thresholds"))
            {
                string[] lines = await System.IO.File.ReadAllLinesAsync("thresholds");
                return View(lines.Select(int.Parse).ToList());
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Thresholds(IList<int> thresholds)
        {
            if (thresholds == null || thresholds.Count < 4)
            {
                ModelState.AddModelError("", "Не все значения введены.");
                return View();
            }

            bool rightOrder = true;
            thresholds.Aggregate((c, n) => {
                if (c >= n)
                    rightOrder = false;
                return n;
            });
            if (!rightOrder)
            {
                ModelState.AddModelError("", "Значения более высоких порогов должны быть больше.");
                return View(thresholds);
            }

            System.IO.File.WriteAllLines("thresholds", thresholds.Select(i => i.ToString()));
            return RedirectToAction("Index", "Home");
        }
    }
}
