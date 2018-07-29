using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebAppRfc.Controllers
{
    public class RoomsController : Controller
    {
        public JsonResult AddRoom(string roomName) {
            Program.Rooms.Add(roomName);
            return new JsonResult(Program.Rooms);
        }

        public JsonResult RemoveRoom(string roomName) {
            Program.Rooms.Remove(roomName);
            return new JsonResult(Program.Rooms);
        }

        public JsonResult GetRooms() {
            return new JsonResult(Program.Rooms);
        }
    }
}