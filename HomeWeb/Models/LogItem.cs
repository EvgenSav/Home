using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace Home.Web.Models
{
    public class LogItem : DatabaseModel<int>, ILogItem
    {
        public DateTime TimeStamp { get; set; }
        public int Cmd { get; set; }
        public int DeviceFk { get; set; }
        public LogItem(DateTime dt, int cmd)
        {
            TimeStamp = dt;
            Cmd = cmd;
        }
    }
}
