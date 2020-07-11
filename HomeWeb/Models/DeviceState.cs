using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Home.Web.Models
{
    [BsonSerializer(typeof(ImpliedImplementationInterfaceSerializer<IDeviceState, DeviceState>))]
    public interface IDeviceState
    {
        int? Bright { get; set; }
        LoadStateEnum LoadState { get; set; }
    }
    public enum LoadStateEnum
    {
        Off = 0,
        On = 1
    }
    public class DeviceState : IDeviceState
    {
        public int? Bright { get; set; }
        public Dictionary<string, double> MeasuredData { get; set; } = null;
        public LoadStateEnum LoadState { get; set; }
        public int FirmwareVersion { get; set; }
        public int ExtType { get; set; }
        public bool IsOffLine { get; set; }
    }
}
