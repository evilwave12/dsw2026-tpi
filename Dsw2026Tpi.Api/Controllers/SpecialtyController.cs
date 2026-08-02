using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/specialties")]
public class SpecialtyController : AppController
{
    private readonly ISpecialtyService _service;
    public SpecialtyController(ISpecialtyService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize, [FromQuery] int pageIndex, [FromQuery] string? name = null)
    {
        var specialties = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(specialties);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task <IActionResult> Add([FromBody] SpecialtyModel.Request specialty)
    {
        var specialties = await _service.Add(specialty);
        return Ok(specialties);
    }

    [HttpPut("{id}")]

    public async Task <IActionResult> Update([FromRoute] Guid id, [FromBody] SpecialtyModel.Request specialty)
    {
        var specialties = await _service.Update(id, specialty);
        return Ok(specialties);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);
        return Ok("ok");
    }
}
