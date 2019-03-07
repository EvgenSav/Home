using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Home.Web.Models;
using MongoDB.Bson;
using Newtonsoft.Json;

namespace Home.Web.Models
{
    public class SensLogItem : LogItem
    {
        public double SensVal { get; set; }

        public SensLogItem(DateTime dt, int cmd, double val) : base(dt, cmd)
        {
            SensVal = val;
        }
        public override string ToString()
        {
            return $"{SensVal:#.##} {(char)176}C";
        }
    }
}
