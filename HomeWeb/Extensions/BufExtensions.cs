using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Driver.Mtrf64;
using Home.Web.Models;

namespace Home.Web.Extensions
{
    public static class BufExtensions
    {
        public static DeviceState GetDeviceState(this Buf buf)
        {
            var state = new DeviceState
            {
                Bright = buf.Bright(),
                ExtType = buf.ExtDevType(),
                FirmwareVersion = buf.FirmwareVer(),
                State = buf.State()
            };
            return state;
        }
    }
}
