using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RFController;

namespace WebAppRfc.Controllers
{
    public class NewDeviceController : Controller
    {
        public static AddNewDev AddNew { get; set; }
        public NewDeviceController() {
            if (AddNew == null) {
                AddNew = new AddNewDev(Program.DevBase, Program.Mtrf64, Program.Rooms);
            }
        }

        public JsonResult Add() {
            AddNew.AddBtnClicked();
            return new JsonResult( new { Device = AddNew.Device, Status = AddNew.Status });
        }

        public JsonResult SendBind() {
            AddNew.SendBind();
            return new JsonResult( new { Status = AddNew.Status });
        }

        public JsonResult RoomSelected(string name, string room, int mode) {
            AddNew.RoomSelected(name, room, mode);
            return new JsonResult(new { Channel = AddNew.FindedChannel, Status = AddNew.Status }); //FindEmptyChannel(mode);
        }

        public IActionResult Index()
        {
            return View();
        }

        public JsonResult GetRooms() {
            return new JsonResult(Program.Rooms);
        }
    }
}