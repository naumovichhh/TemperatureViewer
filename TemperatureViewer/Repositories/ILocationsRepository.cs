using System.Collections.Generic;
using System.Threading.Tasks;
using TemperatureViewer.Models.Entities;

namespace TemperatureViewer.Repositories
{
    public interface ILocationsRepository
    {
        Task<Location> GetByIdAsync(int id, bool loadRelated = false);
        Task<IList<Location>> GetAllAsync(bool loadRelated = false);
        Task CreateAsync(Location sensor);
        Task UpdateAsync(Location user);
        Task DeleteAsync(int id);
    }
}
