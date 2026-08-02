using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Dsw2026Tpi.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/specialities")]
public class SpecialityController : AppController
{
    private readonly ISpecialityService _service;
    public SpecialityController(ISpecialityService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] int pageSize, [FromQuery] int pageIndex, [FromQuery] string? name = null)
    {
        var specialities = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(specialities);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task <IActionResult> Add([FromBody] SpecialityModel.Request speciality)
    {
        var specialities = await _service.Add(speciality);
        return Ok(specialities);
    }

    [HttpPut("{id}")]

    public async Task <IActionResult> Update([FromRoute] Guid id, [FromBody] SpecialityModel.Request speciality)
    {
        var specialities = await _service.Update(id, speciality);
        return Ok(specialities);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);
        return Ok("ok");
    }
}
