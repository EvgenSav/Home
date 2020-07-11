using System.Collections.Generic;

namespace Home.Web.Domain.Automation.Result
{
    public class AutomationResult : IAutomationResult
    {
        public List<IResultItem> ResultItems { get; private set; }

        public void AddResultItem(IResultItem item)
        {
            if(ResultItems == null) ResultItems = new List<IResultItem>();
            ResultItems.Add(item);
        }
        public void RemoveResultItem(IResultItem item)
        {
            ResultItems?.Remove(item);
        }
    }
}
