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
        
        public NewDeviceController() {
            if (Program.AddNew == null) {
                Program.AddNew = new AddNewDev(Program.DevBase, Program.Mtrf64, Program.Rooms);
            }
        }
        public int GetEmptyChannel(int mode) {
            return Program.AddNew.FindedChannel;//FindEmptyChannel(mode);
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