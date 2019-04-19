using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Web.Models
{
    public class BindRequest : DatabaseModel
    {
        public int? DeviceFk { get; set; }
        public DeviceTypeEnum DeviceType { get; set; }
        public string Name { get; set; }
    }
}
