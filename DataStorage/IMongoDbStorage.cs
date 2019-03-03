using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataStorage
{
    public interface IMongoDbStorage
    {
        bool IsConnected { get; }
        Task InsertManyAsync<T>(string collection, IEnumerable<T> items);
        Task<IEnumerable<T>> GetItemsAsync<T>(string collection);
        Task<T> GetByIdAsync<T, TV>(string collection, Expression<Func<T, TV>> getField, TV val);
        Task UpdateByIdAsync<T, TV>(string collection, Expression<Func<T, TV>> getId, T item);
    }
}
