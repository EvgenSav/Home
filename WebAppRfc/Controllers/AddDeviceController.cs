using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RFController;
using WebAppRfc.Models;

namespace WebAppRfc.Controllers
{
    public class AddDeviceController : Controller
    {
        public static AddNewDev AddNew { get; set; }
        public AddDeviceController() {
            if (AddNew == null) {
                AddNew = new AddNewDev(Program.DevBase, Program.Mtrf64, Program.Rooms);
            }
        }

        public JsonResult RoomSelected(NewDevModel newDev) {
            if(newDev != null && newDev.Name != "") {
                AddNew.RoomSelected(newDev);
            }
            return new JsonResult(new { Channel = AddNew.FindedChannel, Status = AddNew.Status });
        }

        public JsonResult SendBind() {
            AddNew.SendBind();
            return new JsonResult(new { Status = AddNew.Status });
        }

        public JsonResult Add() {
            AddNew.SendAdd();
            return new JsonResult( new { Device = AddNew.Device, Status = AddNew.Status });
        }

        

        

        public IActionResult Index()
        {
            return View();
        }

        
    }
}