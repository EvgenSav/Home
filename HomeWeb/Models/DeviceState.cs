using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Web.Models
{
    public class DeviceState
    {
        public int Bright { get; set; }
        public int State { get; set; }
        public int FirmwareVersion { get; set; }
        public int ExtType { get; set; }
        public bool IsOffLine { get; set; }
    }
}
