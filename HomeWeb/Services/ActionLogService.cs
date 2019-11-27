using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStorage;
using Home.Web.Extensions;
using Home.Web.Models;
using Microsoft.Extensions.Caching.Memory;

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

        private async Task<IEnumerable<LogItem>> GetDeviceLogFromDb()
        {
            var log = await _mongoDbStorage.GetItemsAsync<LogItem>(_collection);
            return log;
        }
        public async Task<IEnumerable<LogItem>> GetDeviceLogFromDb(int devId)
        {
            var log = await _mongoDbStorage.FindAsync<LogItem, int>(_collection, r => r.DeviceFk, devId);
            return log;
        }
        public IEnumerable<LogItem> GetDeviceLogFromCache(int devId)
        {
            var log = _memoryCache.GetCollection<LogItem>().Where(r => r.DeviceFk == devId).OrderByDescending(r => r.TimeStamp).ToList();
            return log;
        }
        public async Task<IEnumerable<LogItem>> GetDeviceLog(int devId)
        {
            var log = await _memoryCache.GetCollectionAsync(async () => await GetDeviceLogFromDb(devId), r => r.DeviceFk == devId);
            return log.Where(r => r.DeviceFk == devId).OrderByDescending(r => r.TimeStamp).ToList();
        }

        public async Task AddAsync(LogItem item)
        {
            await _mongoDbStorage.AddAsync(_collection, item);
            _memoryCache.StoreCollectionItem(item);
        }
    }
}
