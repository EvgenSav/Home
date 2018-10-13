using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppRfc.RF;

namespace WebAppRfc.Models {
    public struct NewDevModel {
        public int DevType { get; set; }
        public string Name { get; set; }
        public string Room { get; set; }
    }
}
