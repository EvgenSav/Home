using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAppRfc.Models {
    public interface ILogItem {
        DateTime CurrentTime { get; set; }
        int Cmd { get; set; }
    }
}
