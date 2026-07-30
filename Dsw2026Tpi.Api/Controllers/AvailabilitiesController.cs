using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Dsw2026Tpi.Api.Controllers;


[Route("api/availabilities")]
//[Authorize(Policy = Policies.AdminPolicy)]
public class AvailabilitiesController : AppController
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
        var disponibilidades = await _service.CreateAvailabilitiesAsync(request);
        return Ok(disponibilidades);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromBody] AvailabilityModel.Request request)
    {
        var disponibilidad = await _service.Update(request);
        return Ok(disponibilidad);
    }
}
