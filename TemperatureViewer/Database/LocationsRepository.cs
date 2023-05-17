using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Repositories;

namespace TemperatureViewer.Database
{
    public class LocationsRepository : ILocationsRepository
    {
        private DefaultContext _context;

        public LocationsRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<Location> GetByIdAsync(int id, bool loadRelated = false)
        {
            Location location = await _context.Set<Location>().FindAsync(id);
            if (loadRelated)
            {
                var entry = _context.Entry(location);
                entry.Collection(l => l.Sensors).Load();
            }
            return location;
        }

        public async Task<IList<Location>> GetAllAsync(bool loadRelated = false)
        {
            IList<Location> list;
            if (loadRelated)
            {
                list = await _context.Set<Location>()
                    .Include(l => l.Sensors)
                    .ToListAsync();
            }
            else
            {
                list = await _context.Set<Location>().ToListAsync();
            }

            return list;
        }

        public async Task CreateAsync(Location location)
        {
            await _context.AddAsync(location);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Location location)
        {
            _context.Update(location);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var location = await _context.FindAsync<Location>(id);
            if (location != null)
            {
                _context.Remove(location);
                await _context.SaveChangesAsync();
            }
        }
    }
}
