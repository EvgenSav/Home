using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Home.Web.Models
{
    public interface ILogItem
    {
        [BsonId]
        ObjectId Id { get; set; }
        DateTime TimeStamp { get; set; }
        int Cmd { get; set; }
        int DeviceFk { get; set; }
    }
}
