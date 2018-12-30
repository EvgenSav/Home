using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Home.Web.Models {
    [Serializable]
    public class LogItem : ILogItem {
        public DateTime CurrentTime { get; set; }
        public int Cmd { get; set; }
        public LogItem(DateTime dt, int cmd) {
            CurrentTime = dt;
            Cmd = cmd;
        }
    }
}
