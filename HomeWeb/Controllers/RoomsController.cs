using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace Home.Web.Controllers
{
    public class RoomsController : Controller
    {
        private readonly HomeService homeService;
        public RoomsController(HomeService homeService) {
            this.homeService = homeService;
        }
        public IActionResult AddRoom(string roomName) {
            homeService.Rooms.Add(roomName);
            return Ok(homeService.Rooms);
        }

        public IActionResult RemoveRoom(string roomName) {
            homeService.Rooms.Remove(roomName);
            return Ok(homeService.Rooms);
        }

        public IActionResult GetRooms() {
            return Ok(homeService.Rooms);
        }
    }
}