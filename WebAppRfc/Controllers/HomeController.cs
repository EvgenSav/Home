using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebAppRfc.Models;
using WebAppRfc.Services;
using Driver.Mtrf64;

namespace WebAppRfc.Controllers {
    public class HomeController : Controller {
        private readonly DevicesService devicesService;
        private readonly Mtrf64Context mtrf64Context;
        private readonly ActionHandlerService actionHandlerService;
        public HomeController(DevicesService devicesService, Mtrf64Context mtrf64Context/*, ActionHandlerService actionHandlerService*/) {
            this.devicesService = devicesService;
            this.mtrf64Context = mtrf64Context;
            //this.actionHandlerService = actionHandlerService;
        }
        public IActionResult AngularRoute() {
            return View();
        }

        public IActionResult Index() {
            return View();
        }

        public IActionResult SwitchDev(int devKey) {
            if (devicesService.Devices.ContainsKey(devKey)) {
                devicesService.Devices[devKey].SetSwitch(mtrf64Context);
            }
            return Ok(devicesService.Devices[devKey]);
        }

        public IActionResult SetBright(int devKey, int bright) {
            if (devicesService.Devices.ContainsKey(devKey)) {
                devicesService.Devices[devKey].SetBright(mtrf64Context, bright);
            }
            return Ok();
        }

        public IActionResult DevBase() {
            string res = "";
            foreach (var item in devicesService.Devices) {
                res += String.Format("Name: {0} key: {1} \n", item.Value.Name, item.Key);
            }
            return Ok(devicesService.Devices);
        }

        public IActionResult About() {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact() {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy() {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
