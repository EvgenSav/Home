using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataStorage;
using Home.Web.Domain.Automation;
using Home.Web.Extensions;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;

namespace Home.Web.Services
{
    public class AutomationService : IAutomationService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IMongoDbStorage _dbStorage;
        private readonly NotificationService _notificationService;
        private readonly string automationCollectionName = "automation";
        public AutomationService(IMongoDbStorage dbStorage,  IMemoryCache memoryCache, NotificationService notificationService)
        {
            _dbStorage = dbStorage;
            _memoryCache = memoryCache;
            _notificationService = notificationService;
        }

        public async Task<IAutomationItem> AddAutomation(IAutomationItem automation)
        {
            await _dbStorage.AddAsync(automationCollectionName, automation);
            _memoryCache.StoreCollectionItem(automation);
            return automation;
        }

        public async Task<IAutomationItem> GetAutomationItem(ObjectId id)
        {
            var automations = await GetAutomationItems();
            var auto = automations.FirstOrDefault(r => r.Id == id);
            return auto;
        }
        public async Task UpdateAutomationItem(IAutomationItem item)
        {
            await _dbStorage.UpdateByIdAsync(automationCollectionName, r => r.Id, item);
            await _notificationService.NotifyAll(ActionType.RequestUpdate, item);
            _memoryCache.StoreCollectionItem(item);
        }
        
        public async Task<IEnumerable<IAutomationItem>> GetAutomationItems()
        {
            return await _memoryCache.GetCollectionAsync(async () =>  await GetAutomationItemsFromDb());
        } 

        private async Task<IEnumerable<IAutomationItem>> GetAutomationItemsFromDb()
        {
            return await _dbStorage.GetItemsAsync<IAutomationItem>(automationCollectionName);
        }
    } 
}
