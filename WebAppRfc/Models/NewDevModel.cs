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
        public RfDevice Device { get; set; }
        public BindStatus Status { get; set; }
    }
}
