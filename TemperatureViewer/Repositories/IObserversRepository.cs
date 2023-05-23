using System.Collections.Generic;
using System.Threading.Tasks;
using TemperatureViewer.Models.Entities;

namespace TemperatureViewer.Repositories
{
    public interface IObserversRepository
    {
        Task<Observer> GetByIdAsync(int id, bool loadRelated = false);
        Task<IList<Observer>> GetAllAsync(bool loadRelated = false);
        Task CreateAsync(Observer observer);
        Task UpdateAsync(Observer observer);
        Task DeleteAsync(int id);
    }
}
