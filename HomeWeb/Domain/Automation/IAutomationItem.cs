using Home.Web.Domain.Automation.Condition;
using Home.Web.Domain.Automation.Result;
using Home.Web.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Home.Web.Domain.Automation
{
    [BsonSerializer(typeof(ImpliedImplementationInterfaceSerializer<IAutomationItem, AutomationItem>))]
    public interface IAutomationItem : IDatabaseModel
    {
        string Name { get; }
        ICondition Condition { get; }
        IAutomationResult Result { get; }
    }
}
