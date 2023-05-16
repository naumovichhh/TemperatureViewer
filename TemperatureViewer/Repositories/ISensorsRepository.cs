using System.Collections.Generic;
using System.Threading.Tasks;
using TemperatureViewer.Models.Entities;

namespace TemperatureViewer.Repositories
{
    public interface ISensorsRepository
    {
        Task<Sensor> GetByIdAsync(int id, bool loadRelated = false);
        Task<IList<Sensor>> GetAllAsync(bool loadRelated = false);
        Task CreateAsync(Sensor sensor);
        Task UpdateAsync(Sensor user);
        Task DeleteAsync(int id);

    }
}
