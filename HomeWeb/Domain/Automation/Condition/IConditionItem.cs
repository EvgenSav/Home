using System;
using Home.Web.Models;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;

namespace Home.Web.Domain.Automation.Condition
{
    public enum ConditionTypeEnum
    {
        DeviceCmd,
        DeviceState
    }
    public interface IConditionItem
    {
        Guid Id { get; set; }
        bool IsFulfilled(object args = null);
        ConditionTypeEnum Type { get; }
        int DeviceId { get; }
        string Name { get; set; }
    }
}
