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
        public static void StoreCollection<T>(this IMemoryCache memoryCache, IEnumerable<T> collection)
        {
            var type = typeof(T);
            memoryCache.Set(type.Name, collection);
        }
        public static async Task<IEnumerable<T>> GetCollectionAsync<T>(this IMemoryCache memoryCache, Func<Task<IEnumerable<T>>> factory, Expression<Func<T,bool>> getFromFactory)
        {
            var type = typeof(T);
            var compiled = getFromFactory.Compile();
            var collection = memoryCache.Get<IEnumerable<T>>(type.Name);
            if (collection?.Any(compiled) == true) return collection;
            var factoryItems = await factory();
            using (var entry = memoryCache.CreateEntry(type.Name))
            {
                if (collection != null)
                {
                    var res = collection.Concat(factoryItems);
                    entry.SetValue(res);
                    return res;
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
        public static async Task StoreCollectionItem<T>(this IMemoryCache memoryCache, T item, Func<Task<IEnumerable<T>>> factory) where T : IDatabaseModel
        {
            var type = typeof(T);
            var collection = await memoryCache.GetOrCreateAsync(type.Name,  async r => (await factory()).Append(item));
            var toUpdate = collection.FirstOrDefault(r => r.Id == item.Id);
            if (toUpdate != null)
            {
                toUpdate = item;
                memoryCache.Set(type.Name, collection);
            }
            else
            {
                var updated = collection.Append(item);
                memoryCache.Set(type.Name, updated);
            }
            
        }
        //public static T GetCollectionItem<T>(this IMemoryCache memoryCache, ObjectId id, Func<Task<IEnumerable<T>>> factory, Expression<Func<T,bool>> getFromFactory) where T : IDatabaseModel
        //{
        //    var type = typeof(T);
        //    var compiled = getFromFactory.Compile();
        //    var collection = memoryCache.Get<IEnumerable<T>>(type.Name);
        //    if (collection?.Any(compiled) == true) return collection;
        //    var factoryItems = await factory();
        //    using (var entry = memoryCache.CreateEntry(type.Name))
        //    {
        //        if (collection != null)
        //        {
        //            var res = collection.Concat(factoryItems);
        //            entry.SetValue(res);
        //            return res;
        //        }
        //        entry.SetValue(factoryItems);
        //        return factoryItems;
        //    }
        //}
    }
}
