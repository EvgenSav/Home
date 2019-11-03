using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Domain;
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
        private readonly RequestService _requestService;

        public BindingController(RequestService requestService)
        {
            _requestService = requestService;
        }
        [HttpGet]
        public async Task<IEnumerable<RequestDbo>> GetBindings()
        {
            var bindings = await _requestService.GetBindings();
            return bindings;
        }
        [HttpPost]
        public async Task<RequestDbo> CreateBinding([FromBody] RequestDbo requestDboModel)
        {
            var bindRequest = await _requestService.CreateBindRequest(requestDboModel);
            return bindRequest;
        }
        [HttpPatch("{bindRequestId}")]
        public async Task<RequestDbo> PatchBinding(string bindRequestId, [FromBody] JsonPatchDocument<RequestDbo> patch)
        {
            var bindRequest = await _requestService.GetById(ObjectId.Parse(bindRequestId));
            patch.ApplyTo(bindRequest);
            await _requestService.Update(bindRequest);
            return bindRequest;
        }
        [HttpGet("ExecuteRequest/{bindRequestId}")]
        public async Task<IActionResult> ExecuteBindRequest(string bindRequestId)
        {
            var requestId = ObjectId.Parse(bindRequestId);
            await _requestService.ExecuteRequest(requestId);
            return Ok();
        }
    }
}