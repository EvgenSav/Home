using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStorage;
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

        public async Task<IEnumerable<ILogItem>> GetDeviceLogFromDb(int devId)
        {
            var log = await _mongoDbStorage.FindAsync<ILogItem, int>(_collection, r => r.DeviceFk, devId);
            return log;
        }
        public async Task<IEnumerable<ILogItem>> GetDeviceLog(int devId)
        {
            var log = _memoryCache.Get<IEnumerable<ILogItem>>(string.Empty).Where(r => r.DeviceFk == devId);
            if (log == null || log.Count() == 0)
            {
                log = await GetDeviceLogFromDb(devId);
                _memoryCache.Set(string.Empty, log);
            }
            return log;
        }

        public async Task AddAsync(ILogItem item)
        {
            await _mongoDbStorage.AddAsync(_collection, item);
        }
    }
}
