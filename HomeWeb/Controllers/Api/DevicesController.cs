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
        public async Task<IActionResult> Get()
        {
            var devices = await _devicesService.GetDeviceList();
            return Ok(devices);
        }

        // GET api/<controller>/5
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var device = await _devicesService.GetByIdAsync(id);
            return Ok(device);
        }
        // Patch api/<controller>/5
        [HttpPatch("{id:int}")]
        public async Task<IActionResult> Patch(int id, [FromBody] JsonPatchDocument<Device> patch)
        {
            var device = await _devicesService.GetByIdAsync(id);
            patch.ApplyTo(device);
            await _devicesService.Update(device);
            await _notificationService.NotifyAll(ActionType.UpdateDevice, device);
            return Ok(device);
        }
        [HttpPatch("{devId:int}/settings/{settingType:int}")]
        public async Task<IActionResult> PatchSettings(int devId, int settingType, [FromBody]JsonPatchDocument patch)
        {
            var device = await _devicesService.GetByIdAsync(devId);
            var settings = device.Settings;
            patch.ApplyTo(settings);
            while (settings.GetOperation(out var op))
            {
                await _devicesService.SetNooFSettings(devId, op.SettingType, op.Data);
            }
            //await _devicesService.Update(device);
            return Ok();
        }
        // GET api/<controller>/5
        [HttpGet("{id:int}/settings/{settingType:int}")]
        public async Task<IActionResult> GetSettingsById(int id, int settingType = 16)
        {
            await _devicesService.GetNooFSettings(id, settingType);
            return Ok(id);
        }
        // GET api/<controller>/5
        [HttpGet("Switch/{id}")]
        public async Task<IActionResult> Switch(int id)
        {
            await _devicesService.Switch(id);
            return Ok();
        }
    }
}
