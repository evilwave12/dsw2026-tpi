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

    //GET appointments/patient?dni={dni}
    [HttpGet("patient")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveAppointmentsByPatient([FromQuery] string dni)
    {
        /* ↓this es una validacion pero bueno la dejo comentada por si las dudas↓
        if (string.IsNullOrEmpty(dni)) 
        {
            return BadRequest("el DNI es requerido");
        }
        */
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

}

