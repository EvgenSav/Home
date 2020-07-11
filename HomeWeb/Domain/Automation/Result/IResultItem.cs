using Home.Web.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Home.Web.Domain.Automation.Result
{
    [BsonSerializer(typeof(ImpliedImplementationInterfaceSerializer<IResultItem, ResultItem>))]
    public interface IResultItem
    {
        int DeviceId { get; }
        IDeviceState State { get; }
    }
}
