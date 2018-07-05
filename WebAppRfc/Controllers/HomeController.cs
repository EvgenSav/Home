using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebAppRfc.Models;

namespace WebAppRfc.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public string SwitchDev(int devKey) {
            if(Program.DevBase.Data.ContainsKey(devKey)) {
                Program.DevBase.Data[devKey].SetSwitch(Program.Mtrf64);
            }
            return LastRxMsg();
        }

        public string SetBright(int devKey, int bright) {
            if (Program.DevBase.Data.ContainsKey(devKey)) {
                Program.DevBase.Data[devKey].SetBright(Program.Mtrf64,bright);
            }
            return LastRxMsg();
        }

        public string LastRxMsg() {
            return Program.Mtrf64.GetLogMsg(Program.Mtrf64.rxBuf);
        }
        public string DevBaseList() {
            string res = "";
            foreach (var item in Program.DevBase.Data) {
                res += String.Format("Name: {0} key: {1} \n",item.Value.Name,item.Key);
            }
            return res;
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
