using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Driver.Mtrf64;
using Home.Web.Extensions;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;

namespace Home.Web.Models
{
    public class LogItem : DatabaseModel, ILogItem
    {
        public DateTime TimeStamp { get; set; }
        public int Cmd { get; set; }
        public int DeviceFk { get; set; }
        public int DeviceTypeFk { get; set; }
        public DeviceState State { get; set; }
        public Buf ReceivedBuffer { get; set; }
        public double? MeasuredData { get; set; }
        [JsonConstructor]
        public LogItem() { }

        public LogItem(Buf receivedBuffer, int deviceTypeFk, DeviceState state, double? measuredData = null)
        {
            ReceivedBuffer = receivedBuffer;
            TimeStamp = DateTime.Now;
            Cmd = receivedBuffer.Cmd;
            DeviceFk = receivedBuffer.Id;
            DeviceTypeFk = deviceTypeFk;
            State = state;
            /*switch (device.Type)
            {
                case NooDevType.PowerUnitF:
                    State = new DeviceState
                    {
                        Bright = receivedBuffer.Bright(),
                        ExtType = receivedBuffer.ExtDevType(),
                        FirmwareVersion = receivedBuffer.FirmwareVer(),
                        State = receivedBuffer.State()
                    };
                    break;
                case NooDevType.Sensor:
                    MeasuredData = measuredData;
                    break;

            }*/
        }
        public string GetTemperatureString()
        {
            return $"{MeasuredData:#.##} {(char)176}C";
        }
    }
}
