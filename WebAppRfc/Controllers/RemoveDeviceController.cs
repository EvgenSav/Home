using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WebAppRfc.Hubs;
using WebAppRfc.RF;

namespace WebAppRfc.Controllers
{
    public class RemoveDeviceController : Controller
    {
        public string RemoveDev(int devKey) {
            if (Program.DevBase.Data.ContainsKey(devKey)) {
                Program.DevBase.Data.Remove(devKey);
                FeedbackHub.GlobalContext.Clients.All.SendAsync("RemoveResult", Program.DevBase.Data,"ok");
            }
            return "ok";
        }
        public JsonResult Unbind(int devKey) {
            if(Program.DevBase.Data.ContainsKey(devKey)) {
                switch(Program.DevBase.Data[devKey].Type) {
                    case NooDevType.PowerUnit:
                        Program.DevBase.Data[devKey].Unbind(Program.Mtrf64);
                        break;
                    case NooDevType.PowerUnitF:
                        break;
                }
            }
            return new JsonResult(String.Format("backend Unbind worked. Dev: {0}", devKey));
        }
        public JsonResult Check(int devKey) {
            return new JsonResult(String.Format("backend Check worked. Dev: {0}", devKey));
        }
    }
}