using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAppRfc.Models {
    [Serializable]
    public class PuLogItem : LogItem, ILogItem {
        public int State { get; set; }
        public int Bright { get; set; }

        public PuLogItem(DateTime dt, int cmd, int state, int bright) : base(dt, cmd) {
            State = state;
            Bright = bright;
        }
    }
}
