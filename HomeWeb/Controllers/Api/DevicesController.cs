﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Services;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Home.Web.Controllers.Api
{
    [Route("api/[controller]")]
    public class DevicesController : Controller
    {
        private readonly DevicesService _devicesService;
        public DevicesController(DevicesService devicesService)
        {
            _devicesService = devicesService;
        }
        // GET: api/<controller>
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_devicesService.Devices.Select(p => p.Value));
        }

        // GET api/<controller>/5
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var device = _devicesService.Devices.FirstOrDefault(d => d.Key == id).Value;
            return Ok(device);
        }
        // GET api/<controller>/5
        [HttpGet("{id:int}/settings/{settingType:int}")]
        public IActionResult GetSettingsById(int id, int settingType = 16)
        {
            _devicesService.GetNooFSettings(id, settingType);
            return Ok(id);
        }
        // GET api/<controller>/5
        [HttpGet("Switch/{id}")]
        public IActionResult Switch(int id)
        {
            _devicesService.Switch(id);
            return Ok();
        }

        // POST api/<controller>
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {

        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
