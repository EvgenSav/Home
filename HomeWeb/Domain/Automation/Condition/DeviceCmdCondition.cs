
using Driver.Mtrf64;
using System;

namespace Home.Web.Domain.Automation.Condition
{
    public class DeviceCmdCondition : IConditionItem
    {
        public Guid Id { get; set; }
        public ConditionTypeEnum Type => ConditionTypeEnum.DeviceCmd;
        public int DeviceId { get; set; }
        public int? DeviceCmd { get; set; }
        public string Name { get; set; }

        public bool IsFulfilled(object args)
        {
            var receivedBuffer = args as Buf;
            if (receivedBuffer == null || !DeviceCmd.HasValue) return false;
            if (receivedBuffer.Id != DeviceId || receivedBuffer.Cmd != DeviceCmd) return false;
            return true;
        }
    }
}
