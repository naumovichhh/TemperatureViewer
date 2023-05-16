using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Repositories;

namespace TemperatureViewer.Database
{
    public class ObserversRepository : IObserversRepository
    {
        private DefaultContext _context;

        public ObserversRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<Observer> GetByIdAsync(int id, bool loadRelated = false)
        {
            Observer observer = await _context.Set<Observer>().FindAsync(id);
            if (loadRelated)
            {
                var entry = _context.Entry(observer);
                entry.Collection(o => o.Sensors);
            }
            return observer;
        }

        public async Task<IList<Observer>> GetAllAsync(bool loadRelated = false)
        {
            IList<Observer> list;
            if (loadRelated)
            {
                list = await _context.Set<Observer>().Include(o => o.Sensors).ToListAsync();
            }
            else
            {
                list = await _context.Set<Observer>().ToListAsync();
            }

            return list;
        }

        public async Task CreateAsync(Observer observer)
        {
            await _context.AddAsync(observer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Observer observer)
        {
            _context.Update(observer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            _context.Remove(await _context.FindAsync<Observer>(id));
            await _context.SaveChangesAsync();
        }
    }
}
