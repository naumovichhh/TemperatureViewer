using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Repositories;

namespace TemperatureViewer.Database
{
    public class ThresholdsRepository : IThresholdsRepository
    {
        private DefaultContext _context;

        public ThresholdsRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<Threshold> GetByIdAsync(int id, bool loadRelated = false)
        {
            Threshold threshold = await _context.Set<Threshold>().FindAsync(id);
            if (loadRelated)
            {
                var entry = _context.Entry(threshold);
                entry.Collection(t => t.Sensors);
            }
            return threshold;
        }

        public async Task<IList<Threshold>> GetAllAsync(bool loadRelated = false)
        {
            IQueryable<Threshold> query = _context.Set<Threshold>();
            if (loadRelated)
            {
                query = query.Include(t => t.Sensors);
            }

            return await query.ToListAsync();
        }

        public async Task CreateAsync(Threshold threshold)
        {
            await _context.AddAsync(threshold);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Threshold threshold)
        {
            _context.Update(threshold);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            _context.Remove(await _context.FindAsync<Threshold>(id));
            await _context.SaveChangesAsync();
        }
    }
}
