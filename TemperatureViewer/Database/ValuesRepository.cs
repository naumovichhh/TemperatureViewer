using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Repositories;

namespace TemperatureViewer.Database
{
    public class ValuesRepository : IValuesRepository
    {
        private DefaultContext _context;

        public ValuesRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<Value> GetByIdAsync(int id, bool loadRelated = false)
        {
            Value value = await _context.FindAsync<Value>(id);
            if (loadRelated)
            {
                _context.Entry(value).Reference(v => v.Sensor).Load();
            }
            return value;
        }

        public async Task<IList<Value>> GetFiltered(Expression<Func<Value, bool>> predicate, bool loadRelated = false)
        {
            var query = _context.Set<Value>().Where(predicate);
            if (loadRelated)
                query = query.Include(v => v.Sensor);
            return await query.ToListAsync();
        }

        public async Task<IList<Value>> GetAllAsync(bool loadRelated = false)
        {
            IQueryable<Value> query = _context.Set<Value>();
            if (loadRelated)
                query = query.Include(v => v.Sensor);
            return await query.ToListAsync();
        }

        public async Task CreateAsync(Value value)
        {
            await _context.AddAsync(value);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Value value)
        {
            _context.Update(value);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var value = await _context.FindAsync<Value>(id);
            if (value != null)
            {
                _context.Remove(value);
                await _context.SaveChangesAsync();
            }
        }
    }
}
