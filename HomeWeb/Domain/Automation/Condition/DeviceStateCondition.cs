using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Models;
using MongoDB.Driver;

namespace Home.Web.Domain.Automation.Condition
{
    public class DeviceStateCondition : IConditionItem
    {
        public Guid Id { get; set; }
        public ConditionTypeEnum Type => ConditionTypeEnum.DeviceState;
        public int DeviceId { get; set; }
        public DeviceState  State { get; set; }
        public string Name { get; set; }
        public bool IsFulfilled(object args)
        {
            return false;
        }
    }
}
