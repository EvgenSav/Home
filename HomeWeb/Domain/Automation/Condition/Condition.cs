using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Models;

namespace Home.Web.Domain.Automation.Condition
{
    public class Condition : ICondition
    {
        public List<IConditionItem> ConditionItems { get; set; }
        public List<IConditionRelationItem> RelationItems { get; set; }
        public string Name { get; set; }

        public void AddConditionItem(IConditionItem item)
        {
            if (item == null) return;
            if (ConditionItems == null) ConditionItems = new List<IConditionItem>();
            ConditionItems.Add(item);
        }

        public void AddRelationItem(IConditionRelationItem item)
        {
            throw new NotImplementedException();
        }

        public bool IsFulfilled(object args = null)
        {
            if (ConditionItems == null) return false;
            foreach (var conditionItem in ConditionItems)
            {
                if (!conditionItem.IsFulfilled(args)) return false;
            }
            return true;
        }

        public void RemoveConditionItem(IConditionItem item)
        {
            throw new NotImplementedException();
        }

        public void RemoveRelationItem(IConditionRelationItem item)
        {
            throw new NotImplementedException();
        }
    }
}
