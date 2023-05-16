using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TemperatureViewer.Models.Entities;

namespace TemperatureViewer.Repositories
{
    public interface IValuesRepository
    {
        Task<Value> GetByIdAsync(int id, bool loadRelated = false);
        Task<IList<Value>> GetFiltered(Expression<Func<Value, bool>> predicate, bool loadRelated = false);
        Task<IList<Value>> GetAllAsync(bool loadRelated = false);
        Task CreateAsync(Value value);
        Task UpdateAsync(Value value);
        Task DeleteAsync(int id);
    }
}
