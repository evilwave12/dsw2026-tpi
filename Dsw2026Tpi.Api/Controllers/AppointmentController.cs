using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;

[Route("appointments")]
//[Authorize(Policy = Policies.AdminPolicy)]
public class AppointmentController : AppController
{
    private readonly IAppointmentService _service;

    public AppointmentController(IAppointmentService service)
    {
        _service = service;
    }


    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Add([FromBody] AppointmentModel.Request appointment)
    {
        var appointment2 = await _service.Add(appointment);
        return Ok(appointment2);
    }

  
    [HttpGet("{dni}/patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveAppointmentsByPatient([FromRoute] string dni)
    {
        var appointments = await _service.GetActiveAppointmentsByPatientDni(dni);
        return Ok(appointments);
    }


    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CancelAppointment(Guid id) 
    {
        await _service.CancelAppointment(id);
        return NoContent();
    }

    [HttpGet]
    //[Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDate([FromQuery] int year, [FromQuery] int month, [FromQuery] int day)
    {
        var appointments = await _service.GetByDate(year, month, day);
        return Ok(appointments);
    }


}

