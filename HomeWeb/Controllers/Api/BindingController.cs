using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Models;
using Home.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace Home.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class BindingController : ControllerBase
    {
        private readonly BindingService _bindingService;

        public BindingController(BindingService bindingService)
        {
            _bindingService = bindingService;
        }
        [HttpGet]
        public async Task<IEnumerable<BindRequest>> GetBindings()
        {
            var bindings = await _bindingService.GetBindings();
            return bindings;
        }
        [HttpPost]
        public async Task<BindRequest> CreateBinding([FromBody] BindRequest bindRequestModel)
        {
            var bindRequest = await _bindingService.CreateBindRequest(bindRequestModel);
            return bindRequest;
        }
        [HttpPatch("{bindRequestId}")]
        public async Task<BindRequest> PatchBinding(string bindRequestId, [FromBody] JsonPatchDocument<BindRequest> patch)
        {
            var bindRequest = await _bindingService.GetById(ObjectId.Parse(bindRequestId));
            patch.ApplyTo(bindRequest);
            await _bindingService.Update(bindRequest);
            return bindRequest;
        }
        [HttpGet("ExecuteRequest/{bindRequestId}")]
        public async Task<IActionResult> ExecuteBindRequest(string bindRequestId)
        {
            var requestId = ObjectId.Parse(bindRequestId);
            await _bindingService.ExecuteRequest(requestId);
            return Ok();
        }
    }
}