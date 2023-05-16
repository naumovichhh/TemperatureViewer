using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Repositories;

namespace TemperatureViewer.Database
{
    public class SensorsRepository : ISensorsRepository
    {
        private DefaultContext _context;

        public SensorsRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<Sensor> GetByIdAsync(int id, bool loadRelated = false)
        {
            Sensor sensor = await _context.Set<Sensor>().FindAsync(id);
            if (loadRelated)
            {
                var entry = _context.Entry(sensor);
                entry.Reference(s => s.Location).Load();
                entry.Reference(s => s.Threshold).Load();
                entry.Collection(s => s.Users).Load();
                entry.Collection(s => s.Observers).Load();
            }
            return sensor;
        }

        public async Task<IList<Sensor>> GetAllAsync(bool loadRelated = false)
        {
            IList<Sensor> list;
            if (loadRelated)
            {
                list = await _context.Set<Sensor>()
                    .Include(s => s.Observers)
                    .Include(s => s.Users)
                    .Include(s => s.Location)
                    .Include(s => s.Threshold)
                    .ToListAsync();
            }
            else
            {
                list = await _context.Set<Sensor>().ToListAsync();
            }

            return list;
        }

        public async Task CreateAsync(Sensor sensor)
        {
            await _context.AddAsync(sensor);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Sensor sensor)
        {
            _context.Update(sensor);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            _context.Remove(await _context.FindAsync<Sensor>(id));
            await _context.SaveChangesAsync();
        }
    }
}
