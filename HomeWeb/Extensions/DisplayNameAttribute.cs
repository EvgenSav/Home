using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Home.Web.Extensions
{
    public class DisplayNameAttribute : Attribute
    {
        public string Name { get; set; }

        public DisplayNameAttribute(string displayName)
        {
            Name = displayName;
        }
    }
}
