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
        private readonly string requestCollectionName = "requests";
        public RequestService(DevicesService devicesService, Mtrf64Context mtrf64Context, NotificationService notificationService,
            IMemoryCache memoryCache, IMongoDbStorage mongoDbStorage)
        {
            _devicesService = devicesService;
            _mtrf64Context = mtrf64Context;
            _notificationService = notificationService;
            _memoryCache = memoryCache;
            _mongoDbStorage = mongoDbStorage;
        }

        public async Task<IEnumerable<RequestDbo>> GetRequestList()
        {
            var requests = await _memoryCache.GetCollectionAsync(async ()=> await GetFromDb());
            return requests;
        }

        private async Task<IEnumerable<RequestDbo>> GetFromDb()
        {
            return await _mongoDbStorage.GetItemsAsync<RequestDbo>(requestCollectionName);
        }
        public async Task<RequestDbo> CreateRequest(RequestDbo model)
        {
            await _mongoDbStorage.AddAsync(requestCollectionName, model);
            await _notificationService.NotifyAll(ActionType.RequestAdd, model);
            _memoryCache.StoreCollectionItem(model);
            return model;
        }

        public async Task<RequestDbo> GetById(ObjectId id)
        {
            var requests = await GetRequestList();
            return requests.FirstOrDefault(r => r.Id == id);
        }
        public async Task Update(RequestDbo model)
        {
            await _mongoDbStorage.UpdateByIdAsync(requestCollectionName, r => r.Id, model);
            await _notificationService.NotifyAll(ActionType.RequestUpdate, model);
            _memoryCache.StoreCollectionItem(model);
            
        }
        public async Task Delete(ObjectId id)
        {
            await _mongoDbStorage.DeleteOneAsync<RequestDbo>(requestCollectionName, r => r.Id == id);
            await _notificationService.NotifyAll(ActionType.RequestDelete, id);
            _memoryCache.DeleteCollectionItem<RequestDbo>(r => r.Id == id);
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
