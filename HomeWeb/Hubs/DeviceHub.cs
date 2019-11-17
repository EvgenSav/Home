using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Services;
using Microsoft.AspNetCore.SignalR;

namespace Home.Web.Hubs {
    public class DeviceHub : Hub {

        private readonly DevicesService _deviceService;
        public DeviceHub(DevicesService deviceService) {
            _deviceService = deviceService;
        }
        public override async Task OnConnectedAsync()
        {
            var devices = await _deviceService.GetDeviceList();
            await Clients.All.SendAsync("DeviceCollection", devices);
        }
        public override Task OnDisconnectedAsync(Exception exception) {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
