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
        public static AddDeviceLogic AddDevice { get; set; }
        public AddDeviceController() {
            if (AddDevice == null) {
                AddDevice = new AddDeviceLogic(Program.DevBase, Program.Mtrf64, Program.Rooms);
            }
        }

        public JsonResult StartBind([FromBody] NewDevModel newDev) {
            return Json(AddDevice.StartBind(newDev));
        }

        public JsonResult SendBind() {
            return Json(AddDevice.SendBind());
        }

        public JsonResult Add() {
            return Json(AddDevice.SendAdd());
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