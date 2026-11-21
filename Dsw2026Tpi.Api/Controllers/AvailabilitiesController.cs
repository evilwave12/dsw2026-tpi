using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Dsw2026Tpi.Api.Controllers;


[Route("api/availabilities")]
public class AvailabilitiesController : AppController
{
    private readonly IAvailabilityService _service;

    public AvailabilitiesController(IAvailabilityService service)
    {
        _service = service;
    }

    [HttpPost]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Add([FromBody] AvailabilityModel.Request request)
    {
        var availabilities = await _service.CreateAvailabilitiesAsync(request);
        return Ok(availabilities);
    }

    [HttpPut]
    [Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] AvailabilityModel.Request request)
    {
        var availability = await _service.Update(request);
        return Ok(availability);
    }

    [HttpGet("{id}/availabilitySlots")]
    [Authorize(Policy = Policies.PatientPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailabilitySlots([FromRoute] Guid id)
    {
        var availabilities = await _service.GetAvailabilitySlots(id);
        return Ok(availabilities);
    }
}
