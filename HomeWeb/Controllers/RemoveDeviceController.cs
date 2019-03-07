using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Driver.Mtrf64;
using Home.Web.Extensions;
using Home.Web.Services;
using Home.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Home.Web.Controllers
{
    public class RemoveDeviceController : ControllerBase
    {
        private readonly DevicesService devicesService;
        private readonly Mtrf64Context mtrf64Context;
        private readonly IHubContext<DeviceHub> hubContext;

        public RemoveDeviceController(DevicesService devicesService, Mtrf64Context mtrf64Context, IHubContext<DeviceHub> hubContext)
        {
            this.devicesService = devicesService;
            this.mtrf64Context = mtrf64Context;
            this.hubContext = hubContext;
        }

        public async Task<string> RemoveDev(int devKey)
        {
            var device = await devicesService.GetByIdAsync(devKey);
            if (device != null)
            {
                /*devicesService.Devices.Remove(devKey);
                await hubContext.Clients.All.SendAsync("RemoveResult", devicesService.Devices, "ok");*/
            }
            return await Task.FromResult("ok");
        }
        public async Task<IActionResult> Unbind(int devKey)
        {
            var device = await devicesService.GetByIdAsync(devKey);
            if (device != null)
            {
                switch (device.Type)
                {
                    case NooDevType.PowerUnit:
                        device.Unbind(mtrf64Context);
                        break;
                    case NooDevType.PowerUnitF:
                        break;
                }
            }
            return Ok($"backend Unbind worked. Dev: {devKey}");
        }
        public IActionResult Check(int devKey)
        {
            return Ok($"backend Check worked. Dev: {devKey}");
        }
    }
}