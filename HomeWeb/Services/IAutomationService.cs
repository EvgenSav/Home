using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Domain.Automation;

namespace Home.Web.Services
{
    public interface IAutomationService
    {
        Task<IEnumerable<IAutomationItem>> GetAutomationItems();
    }
}
