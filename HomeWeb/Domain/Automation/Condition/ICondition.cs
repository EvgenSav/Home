using System.Collections.Generic;
using Home.Web.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Home.Web.Domain.Automation.Condition
{
    [BsonSerializer(typeof(ImpliedImplementationInterfaceSerializer<ICondition, Condition>))]
    public interface ICondition
    {
        string Name { get; set; }
        List<IConditionItem> ConditionItems { get; set; }
        List<IConditionRelationItem> RelationItems { get; set; }
        void AddConditionItem(IConditionItem item);
        void RemoveConditionItem(IConditionItem item);
        void AddRelationItem(IConditionRelationItem item);
        void RemoveRelationItem(IConditionRelationItem item);

        bool IsFulfilled(object args = null);
    }
}
