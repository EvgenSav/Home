using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeWeb.Models {
    public interface ILogItem {
        DateTime CurrentTime { get; set; }
        int Cmd { get; set; }
    }
}
