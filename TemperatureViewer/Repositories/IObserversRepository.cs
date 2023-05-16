using System.Collections.Generic;
using System.Threading.Tasks;
using TemperatureViewer.Models.Entities;

namespace TemperatureViewer.Repositories
{
    public interface IObserversRepository
    {
        Task<Observer> GetByIdAsync(int id, bool loadRelated = false);
        Task<IList<Observer>> GetAllAsync(bool loadRelated = false);
        Task CreateAsync(Observer sensor);
        Task UpdateAsync(Observer user);
        Task DeleteAsync(int id);
    }
}
