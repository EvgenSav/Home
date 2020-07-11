using Home.Web.Domain.Automation.Condition;
using Home.Web.Domain.Automation.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Models;

namespace Home.Web.Domain.Automation
{
    public class AutomationItem : DatabaseModel, IAutomationItem
    {
        public string Name { get; set; }
        public ICondition Condition { get; set; }
        public IAutomationResult Result { get; set; }
    }
}
