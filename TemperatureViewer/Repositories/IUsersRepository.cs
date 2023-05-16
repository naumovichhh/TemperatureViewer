using System.Collections.Generic;
using System.Threading.Tasks;
using TemperatureViewer.Models.Entities;

namespace TemperatureViewer.Repositories
{
    public interface IUsersRepository
    {
        Task<User> GetByIdAsync(int id, bool loadRelated = false);
        Task<IList<User>> GetAllAsync(bool loadRelated = false);
        Task CreateAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(int id);

    }
}
