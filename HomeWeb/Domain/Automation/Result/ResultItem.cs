using Home.Web.Models;

namespace Home.Web.Domain.Automation.Result
{
    public class ResultItem : IResultItem
    {
        public int DeviceId { get; set; }

        public IDeviceState State { get; set; }
    }
}
