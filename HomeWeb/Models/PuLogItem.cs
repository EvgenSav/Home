using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace Home.Web.Models
{
    public class PuLogItem : LogItem
    {
        public int State { get; set; }
        public int Bright { get; set; }

        public PuLogItem(DateTime dt, int cmd, int state, int bright) : base(dt, cmd)
        {
            State = state;
            Bright = bright;
        }
    }
}
