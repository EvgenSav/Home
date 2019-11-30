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
    public class RequestController : ControllerBase
    {
        private readonly RequestService _requestService;

        public RequestController(RequestService requestService)
        {
            _requestService = requestService;
        }
        [HttpGet]
        public async Task<IEnumerable<RequestDbo>> GetRequestList()
        {
            var requests = await _requestService.GetRequestList();
            var notCompleted = requests.Where(r => !r.Completed.HasValue).ToList();
            return notCompleted;
        }
        [HttpPost]
        public async Task<RequestDbo> CreateRequest([FromBody] RequestDbo requestDboModel)
        {
            var request = await _requestService.CreateRequest(requestDboModel);
            return request;
        }
        [HttpPatch("{requestId}")]
        public async Task<RequestDbo> PatchRequest(string requestId, [FromBody] JsonPatchDocument<RequestDbo> patch)
        {
            var request = await _requestService.GetById(ObjectId.Parse(requestId));
            patch.ApplyTo(request);
            await _requestService.Update(request);
            return request;
        }
        [HttpDelete("{requestId}")]
        public async Task DeleteRequest(string requestId)
        {
            var id = ObjectId.Parse(requestId);
            await _requestService.Delete(id);
        }
        [HttpGet("ExecuteRequest/{id}")]
        public async Task<IActionResult> ExecuteRequest(string id)
        {
            var requestId = ObjectId.Parse(id);
            await _requestService.ExecuteRequest(requestId);
            return Ok();
        }
    }
}