using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Models.ViewModels;
using TemperatureViewer.Repositories;

namespace TemperatureViewer.Controllers
{
    [Route("Admin/{controller}/{action=Index}/{id?}")]
    [Authorize(Roles = "admin")]
    public class LocationsController : Controller
    {
        //private readonly DefaultContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILocationsRepository _locationsRepository;

        public LocationsController(ILocationsRepository locationsRepository, IWebHostEnvironment environment)
        {
            _locationsRepository = locationsRepository;
            _environment = environment;
        }

        // GET: Locations
        public async Task<IActionResult> Index()
        {
            return View((await _locationsRepository.GetAllAsync()).OrderBy(l => l.Name));//(await _context.Locations.AsNoTracking().OrderBy(l => l.Name).ToListAsync());
        }

        // GET: Locations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _locationsRepository.GetByIdAsync(id.Value);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // GET: Locations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Locations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LocationUploadModel viewModel)
        {
            if (ModelState.IsValid)
            {
                string fileName = UploadFile(viewModel.Image);
                var entity = new Location() { Name = viewModel.Name, Image = fileName };
                await _locationsRepository.CreateAsync(entity);
                //_context.Add(entity);
                //await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: Locations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var entity = await _locationsRepository.GetByIdAsync(id.Value);//_context.Locations.FindAsync(id);
            if (entity == null)
            {
                return NotFound();
            }

            var viewModel = new LocationUploadModel() { Id = entity.Id, Name = entity.Name };
            return View(viewModel);
        }

        // POST: Locations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LocationUploadModel viewModel)
        {
            Location entity = await _locationsRepository.GetByIdAsync(id);//_context.Locations.FirstOrDefaultAsync(m => m.Id == id);
            if (entity == null)
                return NotFound();
            
            string oldFileName = entity.Image;

            if (ModelState.IsValid)
            {
                string fileName = UploadFile(viewModel.Image);
                entity.Name = viewModel.Name;
                entity.Image = fileName;
                try
                {
                    await _locationsRepository.UpdateAsync(entity);
                    //_context.Update(entity);
                    //await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    System.IO.File.Delete(fileName);

                    if (!LocationExists(entity.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                System.IO.File.Delete($"{_environment.WebRootPath}\\img\\{oldFileName}");
                return RedirectToAction(nameof(Index));
            }

            return View(viewModel);
        }

        // GET: Locations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var location = await _locationsRepository.GetByIdAsync(id.Value);//_context.Locations.FirstOrDefaultAsync(m => m.Id == id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // POST: Locations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var location = await _locationsRepository.GetByIdAsync(id);//_context.Locations.FindAsync(id);
            if (location == null)
                return NotFound();

            string fileName = location.Image;
            await _locationsRepository.DeleteAsync(location.Id);
            //_context.Locations.Remove(location);
            //await _context.SaveChangesAsync();
            System.IO.File.Delete(fileName);
            return RedirectToAction(nameof(Index));
        }

        private bool LocationExists(int id)
        {
            return _locationsRepository.GetAllAsync().Result.Any(e => e.Id == id);//_context.Locations.Any(e => e.Id == id);
        }

        private string UploadFile(IFormFile file)
        {
            string fileName = $"{_environment.WebRootPath}\\img\\{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            using (Stream readStream = file.OpenReadStream(), writeStream = System.IO.File.Create(fileName))
            {
                byte[] buffer = new byte[readStream.Length];
                readStream.Read(buffer, 0, buffer.Length);
                writeStream.Write(buffer, 0, buffer.Length);
            }

            return Path.GetFileName(fileName);
        }
    }
}
