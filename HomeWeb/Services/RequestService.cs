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
            var bindRequests = _memoryCache.GetCollection<RequestDbo>();
            if (bindRequests.Any() == false)
            {
                bindRequests = await GetFromDb();
                _memoryCache.StoreCollection(bindRequests);

            }
            return await Task.FromResult(bindRequests);
        }

        private async Task<IEnumerable<RequestDbo>> GetFromDb()
        {
            return await _mongoDbStorage.GetItemsAsync<RequestDbo>(bindingCollectionName);
        }
        public async Task<RequestDbo> CreateBindRequest(RequestDbo model)
        {
            await _mongoDbStorage.AddAsync(bindingCollectionName, model);
            _memoryCache.StoreCollectionItem(model);
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
            _memoryCache.StoreCollectionItem(model);
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
