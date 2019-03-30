using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Home.Web.Models
{
    public interface IDatabaseModel
    {
        ObjectId Id { get; set; }
        bool IsDeleted { get; set; }
    }
}
