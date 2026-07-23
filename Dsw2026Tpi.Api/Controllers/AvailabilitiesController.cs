using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Dsw2026Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilitiesController
    {
        private readonly IAvailabilityService _service;

        public AvailabilitiesController(IAvailabilityService service)
        {
            _service = service;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Add([FromBody] AvailabilityModel.Request request)
        {
            await _service.CreateAvailabilitiesAsync(request);
            return Ok();
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] AvailabilityModel.Request request)
        {
            await _service.UpdateAvailabilitiesAsync(request);
            return Ok();
        }
    }
