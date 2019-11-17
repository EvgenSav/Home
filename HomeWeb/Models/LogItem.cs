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
        public DeviceTypeEnum DeviceTypeFk { get; set; }
        public DeviceState State { get; set; }
        public Buf ReceivedBuffer { get; set; }
        public Dictionary<string, double> MeasuredData { get; set; }
        [JsonConstructor]
        public LogItem() { }

        public LogItem(Buf receivedBuffer, DeviceTypeEnum deviceTypeFk, DeviceState state, Dictionary<string, double> measuredData = null)
        {
            ReceivedBuffer = receivedBuffer;
            TimeStamp = DateTime.Now;
            Cmd = receivedBuffer.Cmd;
            DeviceFk = receivedBuffer.Id;
            DeviceTypeFk = deviceTypeFk;
            State = state;
            if (deviceTypeFk == DeviceTypeEnum.Sensor)
            {
                MeasuredData = measuredData;
            }
            //switch (device.DeviceType)
            //{
            //    case NooDevType.PowerUnitF:
            //        LoadState = new DeviceState
            //        {
            //            Bright = receivedBuffer.Bright(),
            //            ExtType = receivedBuffer.SubType(),
            //            FirmwareVersion = receivedBuffer.FirmwareVer(),
            //            LoadState = receivedBuffer.LoadState()
            //        };
            //        break;
            //}
        }
        public string GetTemperatureString()
        {
            return $"{MeasuredData:#.##} {(char)176}C";
        }
    }
}
