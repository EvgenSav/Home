using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Home.Driver.Mtrf64;
using Home.Web.Extensions;
using Home.Web.Services;
using Home.Web.Hubs;

namespace HomeWeb.Controllers
{
    public class RemoveDeviceController : ControllerBase
    {
        private readonly DevicesService devicesService;
        private readonly Mtrf64Context mtrf64Context;
        private readonly IHubContext<FeedbackHub> hubContext;

        public RemoveDeviceController(DevicesService devicesService, Mtrf64Context mtrf64Context, IHubContext<FeedbackHub> hubContext) {
            this.devicesService = devicesService;
            this.mtrf64Context = mtrf64Context;
            this.hubContext = hubContext;
        }

        public string RemoveDev(int devKey) {
            if (devicesService.Devices.ContainsKey(devKey)) {
                devicesService.Devices.Remove(devKey);
                hubContext.Clients.All.SendAsync("RemoveResult", devicesService.Devices, "ok");
            }
            return "ok";
        }
        public IActionResult Unbind(int devKey) {
            if(devicesService.Devices.ContainsKey(devKey)) {
                switch(devicesService.Devices[devKey].Type) {
                    case NooDevType.PowerUnit:
                        devicesService.Devices[devKey].Unbind(mtrf64Context);
                        break;
                    case NooDevType.PowerUnitF:
                        break;
                }
            }
            return Ok(string.Format("backend Unbind worked. Dev: {0}", devKey));
        }
        public IActionResult Check(int devKey) {
           return Ok(string.Format("backend Check worked. Dev: {0}", devKey));
        }
    }
}