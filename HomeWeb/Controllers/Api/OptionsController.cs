using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Extensions;
using Home.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Home.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class OptionsController : ControllerBase
    {
        [Route("General")]
        public IActionResult GetGeneralOptions()
        {
            var deviceTypeOptions = EnumExtensions.GetEnumOptions<DeviceTypeEnum>();
            return Ok(new
            {
                deviceTypeOptions
            });
        }
    }
}