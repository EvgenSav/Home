using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppRfc.RF;

namespace WebAppRfc.Models {
    public class NewDevModel {
        public int DevType { get; set; }
        public string Name { get; set; }
        public string Room { get; set; }
    }
    public class BindModel {
        public BindModel(RfDevice devicce, BindStatus status) {
            Device = devicce != null ? devicce : new RfDevice();
            Status = status != null ? status : new BindStatus();
        }
        public RfDevice Device { get; set; }
        public BindStatus Status { get; set; }
    }
}
