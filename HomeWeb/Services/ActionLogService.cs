using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStorage;
using Home.Web.Extensions;
using Home.Web.Models;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;

namespace Home.Web.Services
{
    public class ActionLogService
    {
        private readonly IMongoDbStorage _mongoDbStorage;
        private readonly IMemoryCache _memoryCache;
        private static readonly string _collection = "action_log";

        public ActionLogService(IMongoDbStorage mongoDbStorage, IMemoryCache memoryCache)
        {
            _mongoDbStorage = mongoDbStorage;
            _memoryCache = memoryCache;
        }

        public IEnumerable<LogItem> GetDeviceLogFromCache(int devId)
        {
            var log = _memoryCache.GetCollection<LogItem>().Where(r => r.DeviceFk == devId).OrderByDescending(r => r.TimeStamp).ToList();
            return log;
        }
        public async Task<IEnumerable<LogItem>> GetDeviceLog(int devId)
        {
            var log = await GetDeviceLogFromDb(devId);
            return log.Where(r => r.DeviceFk == devId).OrderByDescending(r => r.TimeStamp).ToList();
        }

        public async Task AddAsync(LogItem item)
        {
            await _mongoDbStorage.AddAsync(_collection, item);
            _memoryCache.StoreCollectionItem(item);
        }

        public async Task<IEnumerable<LogItem>> GetDeviceLogByDate(int devId, DateTime? date = null)
        {
            var filters = new List<FilterDefinition<LogItem>>();
            filters.Add(Builders<LogItem>.Filter.Where(r => r.DeviceFk == devId));
            if (date.HasValue)
            {
                var left = date.Value.Date;
                var right = date.Value.Date.AddDays(1).AddSeconds(-1);
                var andFilter = new FilterDefinitionBuilder<LogItem>()
                    .And(Builders<LogItem>.Filter.Gt(r => r.TimeStamp, left), Builders<LogItem>.Filter.Lt(r => r.TimeStamp, right));
                filters.Add(andFilter);
            }
            var filterDefinition = new FilterDefinitionBuilder<LogItem>().And(filters);
            var items = await _mongoDbStorage.FindWhere<LogItem>(_collection, filterDefinition);
            return items;
        }

        private async Task<IEnumerable<LogItem>> GetAllDevicesLogFromDb()
        {
            var log = await _mongoDbStorage.GetItemsAsync<LogItem>(_collection);
            return log;
        }
        private async Task<IEnumerable<LogItem>> GetDeviceLogFromDb(int devId)
        {
            var log = await _mongoDbStorage.FindAsync<LogItem, int>(_collection, r => r.DeviceFk, devId);
            return log;
        }
    }
}
