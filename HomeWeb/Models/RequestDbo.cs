using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Domain;

namespace Home.Web.Models
{
    public enum RequestStepEnum
    {
        Created,
        Pending,
        Completed,
        Error
    }
    public class RequestDbo : DatabaseModel
    {
        public MetaData MetaData { get; set; }
        public DateTime? Completed { get; set; }
        public RequestTypeEnum Type { get; set; }
        public DeviceTypeEnum DeviceType { get; set; }
        public string Name { get; set; }
        public RequestStepEnum Step { get; set; } = RequestStepEnum.Created;
    }
}
