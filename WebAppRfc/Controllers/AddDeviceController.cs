using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebAppRfc.RF;
using WebAppRfc.Models;
using WebAppRfc.Logics;

namespace WebAppRfc.Controllers
{
    public class AddDeviceController : Controller
    {
        public static AddNewDevLogic AddNew { get; set; }
        public AddDeviceController() {
            if (AddNew == null) {
                AddNew = new AddNewDevLogic(Program.DevBase, Program.Mtrf64, Program.Rooms);
            }
        }

        public JsonResult StartBind([FromBody] NewDevModel newDev) {
            if(newDev != null && newDev.Name != "") {
                AddNew.StartBind(newDev);
            }
            return new JsonResult(new { Status = AddNew.Status });
        }

        public JsonResult SendBind() {
            AddNew.SendBind();
            return new JsonResult(new { Status = AddNew.Status });
        }

        public JsonResult Add() {
            AddNew.SendAdd();
            return new JsonResult( new { Device = AddNew.Device, Status = AddNew.Status });
        }

        public OkResult CheckBind([FromBody] RfDevice dev) {
            dev.SetSwitch(Program.Mtrf64);
            return new OkResult();
        }
        public OkResult CancelBind([FromBody] RfDevice dev) {
            dev.Unbind(Program.Mtrf64);
            return new OkResult();
        } 
    }
}