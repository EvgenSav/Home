using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Home.Web.Models;

namespace Home.Web.Models {
    [Serializable]
    public class SensLogItem : LogItem, ILogItem {
        public float SensVal { get; set; }
        public SensLogItem(DateTime dt, int cmd, float val) : base(dt, cmd) {
            SensVal = val;
        }
        public override string ToString() {
            return string.Format("{0:#.##} {1}C", SensVal, (char)176);
        }
    }
}
