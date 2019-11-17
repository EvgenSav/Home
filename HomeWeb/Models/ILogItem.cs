using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Driver.Mtrf64;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Home.Web.Models
{
    [BsonSerializer(typeof(ImpliedImplementationInterfaceSerializer<ILogItem, LogItem>))]
    public interface ILogItem : IDatabaseModel
    {
        DateTime TimeStamp { get; set; }
        int Cmd { get; set; }
        int DeviceFk { get; set; }
        DeviceTypeEnum DeviceTypeFk { get; set; }
        DeviceState State { get; set; }
        Buf ReceivedBuffer { get; set; }
        Dictionary<string, double> MeasuredData { get; set; }
    }
}
