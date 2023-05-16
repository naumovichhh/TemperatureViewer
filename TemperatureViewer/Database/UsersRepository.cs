using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TemperatureViewer.Models.Entities;
using TemperatureViewer.Repositories;

namespace TemperatureViewer.Database
{
    public class UsersRepository : IUsersRepository
    {
        private DefaultContext _context;
        public UsersRepository(DefaultContext context)
        {
            _context = context;
        }

        public async Task<User> GetByIdAsync(int id, bool loadRelated = false)
        {
            User user = await _context.Set<User>().FindAsync(id);
            if (loadRelated)
            {
                await _context.Entry(user).Collection(u => u.Sensors).LoadAsync();
            }
            return user;
        }

        public async Task<IList<User>> GetAllAsync(bool loadRelated = false)
        {
            IList<User> result;
            if (loadRelated)
                result = await _context.Set<User>().Include(u => u.Sensors).ToListAsync();
            else
                result = await _context.Set<User>().ToListAsync();
            return result;
        }

        public async Task CreateAsync(User user)
        {
            await _context.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(User user)
        {
            _context.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            _context.Remove(await _context.Set<User>().FindAsync(id));
            await _context.SaveChangesAsync();
        }
    }
}
