using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Home.Web.Domain.Automation.Result
{
    [BsonSerializer(typeof(ImpliedImplementationInterfaceSerializer<IAutomationResult, AutomationResult>))]
    public interface IAutomationResult
    {
        List<IResultItem> ResultItems { get; }
        void AddResultItem(IResultItem item);
        void RemoveResultItem(IResultItem item);
    }
}
