using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Web.Models {
    public class NewDevModel {
        public DeviceTypeEnum DevType { get; set; }
        public string Name { get; set; }
        public string Room { get; set; }
    }
}
