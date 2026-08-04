using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("api/doctors")]
[Authorize(Policy = Policies.AdminPolicy)]
public class DoctorController : AppController
{
    private readonly IDoctorService _service;

    public DoctorController(IDoctorService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery]int pageSize, [FromQuery]int pageIndex, [FromQuery]string? name = null)
    {
        var doctors = await _service.GetAll(pageSize, pageIndex, name);
        return Ok(doctors);
    }

    [HttpGet("{id}/availabilities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailabilitiesByDoctorsId([FromRoute] Guid id)
    {
        var availabilities = await _service.GetAvailabilities(id);
        return Ok(availabilities);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> Add([FromBody] DoctorModel.Request doctor)
    {
        var doctor2 = await _service.Add(doctor);
        return Ok(doctor2);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] DoctorModel.Request doctor)
    {
        var doctor2 = await _service.Update(id, doctor);
        return Ok(doctor2);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.Delete(id);
        return Ok("ok");
    }
}
