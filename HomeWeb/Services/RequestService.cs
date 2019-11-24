using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using DataStorage;
using Driver.Mtrf64;
using Home.Web.Domain;
using Home.Web.Extensions;
using Home.Web.Models;
using Home.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;


namespace Home.Web.Services
{
    public class RequestService
    {
        private readonly DevicesService _devicesService;
        private readonly Mtrf64Context _mtrf64Context;
        private readonly NotificationService _notificationService;
        private readonly IMemoryCache _memoryCache;
        private readonly IMongoDbStorage _mongoDbStorage;
        private readonly string bindingCollectionName = "bindings";
        public RequestService(DevicesService devicesService, Mtrf64Context mtrf64Context, NotificationService notificationService,
            IMemoryCache memoryCache, IMongoDbStorage mongoDbStorage)
        {
            _devicesService = devicesService;
            _mtrf64Context = mtrf64Context;
            _notificationService = notificationService;
            _memoryCache = memoryCache;
            _mongoDbStorage = mongoDbStorage;
        }

        public async Task<IEnumerable<RequestDbo>> GetBindings()
        {
            var bindRequests = await _memoryCache.GetCollectionAsync(async ()=> await GetFromDb());
            return bindRequests;
        }

        private async Task<IEnumerable<RequestDbo>> GetFromDb()
        {
            return await _mongoDbStorage.GetItemsAsync<RequestDbo>(bindingCollectionName);
        }
        public async Task<RequestDbo> CreateBindRequest(RequestDbo model)
        {
            await _mongoDbStorage.AddAsync(bindingCollectionName, model);
            await _memoryCache.StoreCollectionItem(model, () => GetFromDb());
            return model;
        }

        public async Task<RequestDbo> GetById(ObjectId id)
        {
            var bindings = await GetBindings();
            return bindings.FirstOrDefault(r => r.Id == id);
        }
        public async Task Update(RequestDbo model)
        {
            await _mongoDbStorage.UpdateByIdAsync(bindingCollectionName, r => r.Id, model);
            await _memoryCache.StoreCollectionItem(model, () => GetFromDb());
        }
        public async Task Delete(RequestDbo model)
        {
            await _mongoDbStorage.DeleteOneAsync<RequestDbo>(bindingCollectionName, r => r.Id == model.Id);
            _memoryCache.DeleteCollectionItem<RequestDbo>(r => r.Id == model.Id);
        }

        public async Task ExecuteRequest(ObjectId requestId)
        {
            var requestDbo = await GetById(requestId);
            var request = Request.FromDbo(requestDbo, _mtrf64Context);
            var processor = new ActionProcessor(_mtrf64Context, _devicesService, _notificationService, this);
            await processor.Process(request);
        }
    }
}
