using Home.Web.Models;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Home.Web.Extensions
{
    public static class MemoryCacheExtensions
    {
        public static IEnumerable<T> GetCollection<T>(this IMemoryCache memoryCache)
        {
            var type = typeof(T);
            var collection = memoryCache.Get<IEnumerable<T>>(type.Name)?.ToList() ?? new List<T>();
            return collection;
        }
        public static async Task<IEnumerable<T>> GetCollectionAsync<T>(this IMemoryCache memoryCache, Func<Task<IEnumerable<T>>> factory, Expression<Func<T,bool>> getFromFactory)
        {
            var type = typeof(T);
            var compiled = getFromFactory.Compile();
            var collection = memoryCache.Get<IEnumerable<T>>(type.Name)?.ToList();
            if (collection?.Any(compiled) == true) return collection;
            var factoryItems = await factory();
            using (var entry = memoryCache.CreateEntry(type.Name))
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                if (collection != null)
                {
                    collection.AddRange(factoryItems);
                    // entry.SetValue(collection); redundant => AddRange already modified collection in memory
                    return collection;
                }
                entry.SetValue(factoryItems);
                return factoryItems;
            }
        }
        public static async Task<IEnumerable<T>> GetCollectionAsync<T>(this IMemoryCache memoryCache, Func<Task<IEnumerable<T>>> factory)
        {
            var type = typeof(T);
            return await memoryCache.GetOrCreateAsync(type.Name, async r => await factory());
        }

        public static void StoreCollectionItem<T>(this IMemoryCache memoryCache, T item) where T : IDatabaseModel
        {
            var type = typeof(T);
            var collection = memoryCache.Get<IEnumerable<T>>(type.Name)?.ToList();
            if (collection != null)
            {
                var exists = collection.FirstOrDefault(r => r.Id == item.Id);
                if (exists != null) exists = item;
                else collection.Add(item);
                memoryCache.Set(type.Name, collection, TimeSpan.FromMinutes(5));
                return;
            }
            using (var entry = memoryCache.CreateEntry(type.Name))
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                entry.SetValue(new List<T> {item});
            }
        }
        public static void DeleteCollectionItem<T>(this IMemoryCache memoryCache, Func<T, bool> predicate) where T : IDatabaseModel
        {
            var type = typeof(T);
            var collection = memoryCache.Get<IEnumerable<T>>(type.Name)?.ToList();
            if (collection?.Any(predicate) != true) return;
            using (var entry = memoryCache.CreateEntry(type.Name))
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                var toDelete = collection.FirstOrDefault(predicate);
                collection.Remove(toDelete);
                entry.SetValue(collection);
            }
        }
        public static void DeleteCollectionItems<T>(this IMemoryCache memoryCache, Func<T, bool> predicate) where T : IDatabaseModel
        {
            var type = typeof(T);
            var collection = memoryCache.Get<IEnumerable<T>>(type.Name)?.ToList();
            if (collection?.Any(predicate) != true) return;
            using (var entry = memoryCache.CreateEntry(type.Name))
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

                    var toBeDeleted = collection.Where(predicate);
                    var res = collection.Except(toBeDeleted).ToList();
                    entry.SetValue(res);
            }
        }
    }
}
