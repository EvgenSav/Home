using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Models;
using Home.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Home.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActionLogController : ControllerBase
    {
        private readonly ActionLogService _actionLogService;
        public ActionLogController(ActionLogService actionLogService)
        {
            _actionLogService = actionLogService;
        }
        [HttpGet("{devId:int}")]
        public async Task<IEnumerable<ILogItem>> GetLogs(int devId)
        {
            return await _actionLogService.GetDeviceLog(devId);
        }
        [HttpGet("{devId:int}/From/{dateIsoString}")]
        public async Task<IEnumerable<ILogItem>> GetLogsByDate(int devId, string dateIsoString = null)
        {
            DateTime.TryParse(dateIsoString, out var date);
            return await _actionLogService.GetDeviceLogByDate(devId, date);
        }
    }
}