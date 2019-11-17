using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Web.Models
{
    public enum LoadStateEnum
    {
        Off = 0,
        On = 1
    }
    public class DeviceState
    {
        public int? Bright { get; set; }
        public Dictionary<string, double> MeasuredData { get; set; } = null;
        public LoadStateEnum LoadState { get; set; }
        public int FirmwareVersion { get; set; }
        public int ExtType { get; set; }
        public bool IsOffLine { get; set; }
    }
}
