using System.Collections.Generic;
using System.Threading.Tasks;
using TemperatureViewer.Models.Entities;

namespace TemperatureViewer.Repositories
{
    public interface IThresholdsRepository
    {
        Task<Threshold> GetByIdAsync(int id, bool loadRelated = false);
        Task<IList<Threshold>> GetAllAsync(bool loadRelated = false);
        Task CreateAsync(Threshold threshold);
        Task UpdateAsync(Threshold threshold);
        Task DeleteAsync(int id);
    }
}
