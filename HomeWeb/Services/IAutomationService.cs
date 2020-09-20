using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Domain.Automation;
using MongoDB.Bson;

namespace Home.Web.Services
{
    public interface IAutomationService
    {
        Task<IEnumerable<IAutomationItem>> GetAutomationItems();
        Task<IAutomationItem> GetAutomationItem(ObjectId id);
        Task UpdateAutomationItem(IAutomationItem item);
        Task<IAutomationItem> AddAutomation(IAutomationItem automation);
    }
}
