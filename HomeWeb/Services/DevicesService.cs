using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using DataStorage;
using Driver.Mtrf64;
using Home.Web.Extensions;
using Home.Web.Models;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Driver;


namespace Home.Web.Services
{
    public class DevicesService
    {
        private readonly Mtrf64Context mtrf64Context;
        private readonly IMongoDbStorage _mongoDbStorage;
        private readonly IMemoryCache _memoryCache;

        private static readonly string _collection = "devices";

        public DevicesService(Mtrf64Context mtrf64Context, IMongoDbStorage mongoDbStorage, IMemoryCache memoryCache)
        {
            _mongoDbStorage = mongoDbStorage;
            _memoryCache = memoryCache;
            this.mtrf64Context = mtrf64Context;
        }
        public async Task<IEnumerable<RfDevice>> GetDeviceList()
        {
            var devices = _memoryCache.Get<IEnumerable<RfDevice>>(string.Empty);
            if (devices == null || devices.FirstOrDefault() == null)
            {
                devices = await GetFromDb();
                _memoryCache.Set(string.Empty, devices);
            }
            return await Task.FromResult(devices);
        }
        public async Task<RfDevice> GetByIdAsync(int deviceKey)
        {
            var devices = await GetDeviceList();
            var device = devices.FirstOrDefault(r => r.Key == deviceKey);
            return device;
        }

        public async Task<IEnumerable<RfDevice>> GetFromDb()
        {
            return await _mongoDbStorage.GetItemsAsync<RfDevice>(_collection);
        }
        public async Task<IEnumerable<T>> ImportDeviceList<T>(IEnumerable<T> devices)
        {
            await _mongoDbStorage.AddManyAsync(_collection, devices);
            return devices;
        }

        public async Task Update(RfDevice device)
        {
            await _mongoDbStorage.UpdateByIdAsync("devices", r => r.Key, device);
        }
        public async Task GetNooFSettings(int devId, int settingType)
        {
            var dev = await GetByIdAsync(devId);
            dev.GetNooFSettings(mtrf64Context, settingType);
        }
        public async Task SetNooFSettings(int devId, NooFSettingType settingType, int settings)
        {
            var dev = await GetByIdAsync(devId);
            dev.SetNooFSettings(mtrf64Context, settingType, settings);
        }
        public async Task Switch(int devId)
        {
            var dev = await GetByIdAsync(devId);
            dev.SetSwitch(mtrf64Context);
        }
    }
}
