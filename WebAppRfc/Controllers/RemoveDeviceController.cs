using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace WebAppRfc.Controllers
{
    public class RemoveDeviceController : Controller
    {
        public JsonResult RemoveDev(int devKey) {
            return new JsonResult(String.Format("backend meth. worked. Dev: {0}", devKey));
        }
    }
}