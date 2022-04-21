using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Data;
using TemperatureViewer.Models;

namespace TemperatureViewer.Controllers
{
    public class ObserversController : Controller
    {
        private readonly DefaultContext _context;

        public ObserversController(DefaultContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Observers.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Email")] Observer observer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(observer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(observer);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var observer = await _context.Observers
                .FirstOrDefaultAsync(m => m.Email == id);
            if (observer == null)
            {
                return NotFound();
            }

            return View(observer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var observer = await _context.Observers.FindAsync(id);
            _context.Observers.Remove(observer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ObserverExists(string id)
        {
            return _context.Observers.Any(e => e.Email == id);
        }
    }
}
