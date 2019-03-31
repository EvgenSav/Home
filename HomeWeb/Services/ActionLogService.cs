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

        public async Task<IEnumerable<LogItem>> GetDeviceLogFromDb(int devId)
        {
            var log = await _mongoDbStorage.FindAsync<LogItem, int>(_collection, r => r.DeviceFk, devId);
            return log;
        }
        public async Task<IEnumerable<LogItem>> GetDeviceLog(int devId)
        {
            var log = _memoryCache.GetCollection<LogItem>().ToList();
            if (log.Any(r => r.DeviceFk == devId) == false)
            {
                var devLog = await GetDeviceLogFromDb(devId);
                log.AddRange(devLog);
                _memoryCache.StoreCollection(log);
            }
            return log.Where(r => r.DeviceFk == devId).ToList();
        }

        public async Task AddAsync(LogItem item)
        {
            await _mongoDbStorage.AddAsync(_collection, item);
            _memoryCache.StoreCollectionItem(item);
        }
    }
}
