using System.Collections.Generic;
using System.Threading.Tasks;
using TemperatureViewer.Models.Entities;

namespace TemperatureViewer.Repositories
{
    public interface IUsersRepository
    {
        public Task<User> GetByIdAsync(int id, bool loadRelated = false);
        public Task<IList<User>> GetAllAsync(bool loadRelated = false);
        public Task CreateAsync(User user);
        public Task UpdateAsync(User user);
        public Task DeleteAsync(int id);

    }
}
