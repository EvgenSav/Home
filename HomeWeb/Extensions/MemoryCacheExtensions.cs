using Home.Web.Models;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Web.Extensions
{
    public static class MemoryCacheExtensions
    {
        public static void StoreCollection<T>(this IMemoryCache memoryCache, IEnumerable<T> collection)
        {
            var type = typeof(T);
            memoryCache.Set(type.Name, collection);
        }
        public static IEnumerable<T> GetCollection<T>(this IMemoryCache memoryCache)
        {
            var type = typeof(T);
            return memoryCache.GetOrCreate(type.Name, r => new List<T>());
        }
        public static void StoreCollectionItem<T>(this IMemoryCache memoryCache, T item) where T : IDatabaseModel
        {
            var type = item.GetType();
            var collection = memoryCache.GetOrCreate(type.Name, r => new List<T>());
            var toUpdate = collection.FirstOrDefault(r => r.Id == item.Id);
            if (toUpdate != null)
            {
                toUpdate = item;
            }
            else collection.Add(item);
            memoryCache.Set(type.Name, collection);
        }
        public static T GetCollectionItem<T>(this IMemoryCache memoryCache, ObjectId id) where T : IDatabaseModel
        {
            var type = typeof(T);
            var collection = memoryCache.GetOrCreate(type.Name, r => new List<T>());
            var item = collection.FirstOrDefault(r => r.Id == id);
            return item;
        }
    }
}
