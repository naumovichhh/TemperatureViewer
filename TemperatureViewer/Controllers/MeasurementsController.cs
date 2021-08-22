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
    public class MeasurementsController : Controller
    {
        private readonly DefaultContext _context;

        public MeasurementsController(DefaultContext context)
        {
            _context = context;
        }

        // GET: Measurements
        public async Task<IActionResult> Index()
        {
            var defaultContext = _context.Measurements.Include(m => m.Termometer);
            return View(await defaultContext.ToListAsync());
        }

        // GET: Measurements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var measurement = await _context.Measurements
                .Include(m => m.Termometer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (measurement == null)
            {
                return NotFound();
            }

            return View(measurement);
        }

        // GET: Measurements/Create
        public IActionResult Create()
        {
            ViewData["TermometerId"] = new SelectList(_context.Termometers, "Id", "Id");
            return View();
        }

        // POST: Measurements/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TermometerId,MeasurementTime,Temperature")] Measurement measurement)
        {
            if (ModelState.IsValid)
            {
                _context.Add(measurement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["TermometerId"] = new SelectList(_context.Termometers, "Id", "Id", measurement.TermometerId);
            return View(measurement);
        }

        // GET: Measurements/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var measurement = await _context.Measurements.FindAsync(id);
            if (measurement == null)
            {
                return NotFound();
            }
            ViewData["TermometerId"] = new SelectList(_context.Termometers, "Id", "Id", measurement.TermometerId);
            return View(measurement);
        }

        // POST: Measurements/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var measurementToUpdate = await _context.Measurements.FirstOrDefaultAsync(s => s.Id == id);
            if (await TryUpdateModelAsync<Measurement>(
                measurementToUpdate,
                "",
                m => m.Temperature, m => m.MeasurementTime))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
            return View(measurementToUpdate);
        }

        // GET: Measurements/Delete/5
        public async Task<IActionResult> Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return NotFound();
            }

            var measurement = await _context.Measurements
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);
            if (measurement == null)
            {
                return NotFound();
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] =
                    "Delete failed. Try again, and if the problem persists " +
                    "see your system administrator.";
            }

            return View(measurement);
        }

        // POST: Measurements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var measurement = await _context.Measurements.FindAsync(id);
            if (measurement == null)
            {
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.Measurements.Remove(measurement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                return RedirectToAction(nameof(Delete), new { id = id, saveChanesError = true });
            }
        }

        private bool MeasurementExists(int id)
        {
            return _context.Measurements.Any(e => e.Id == id);
        }
    }
}
