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
        Task AddAsync<T>(string collection, T item);
        Task AddManyAsync<T>(string collection, IEnumerable<T> items);
        Task<IEnumerable<T>> GetItemsAsync<T>(string collection);
        Task<IEnumerable<T>> FindAsync<T, TV>(string collection, Expression<Func<T, TV>> getField, TV fieldVal);
        Task UpdateByIdAsync<T, TV>(string collection, Expression<Func<T, TV>> getId, T item);
        Task DeleteOneAsync<T>(string collection, Expression<Func<T, bool>> predicate);
    }
}
