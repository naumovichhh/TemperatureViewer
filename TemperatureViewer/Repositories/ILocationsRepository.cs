using System.Collections.Generic;
using System.Threading.Tasks;
using TemperatureViewer.Models.Entities;

namespace TemperatureViewer.Repositories
{
    public interface ILocationsRepository
    {
        Task<Location> GetByIdAsync(int id, bool loadRelated = false);
        Task<IList<Location>> GetAllAsync(bool loadRelated = false);
        Task CreateAsync(Location location);
        Task UpdateAsync(Location location);
        Task DeleteAsync(int id);
    }
}
