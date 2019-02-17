using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Models;
using Home.Web.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Home.Web.Controllers.Api
{
    [Route("api/[controller]")]
    public class DevicesController : Controller
    {
        private readonly DevicesService _devicesService;
        private readonly NotificationService _notificationService;
        public DevicesController(DevicesService devicesService, NotificationService notificationService)
        {
            _devicesService = devicesService;
            _notificationService = notificationService;
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
        // Patch api/<controller>/5
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<RfDevice> patch)
        {
            var device = _devicesService.Devices.FirstOrDefault(d => d.Key == id).Value;
            patch.ApplyTo(device);
            await _devicesService.Update();
            await _notificationService.NotifyAll(ActionType.UpdateDevice, device);
            return Ok(device);
        }
        [HttpPatch("{devId:int}/settings/{settingType:int}")]
        public async Task<IActionResult> PatchSettings(int devId, int settingType, [FromBody]JsonPatchDocument patch)
        {
            var settings = _devicesService.Devices[devId].Settings;
            patch.ApplyTo(settings);
            while (settings.GetOperation(out var op))
            {
                _devicesService.SetNooFSettings(devId, op.SettingType, op.Data);
            }
            await _devicesService.Update();
            return Ok();
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
