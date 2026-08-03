using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2026Tpi.Api.Controllers;


[Route("api/appointments")]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelAppointment(Guid id) 
    {
        await _service.CancelAppointment(id);
        return Ok("ok");
    }

    [HttpGet]
    //[Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByDate([FromQuery] DateOnly date)
    {
        var appointments = await _service.GetByDate(date);
        return Ok(appointments);
    }

    [HttpGet("/search")]
    //[Authorize(Policy = Policies.AdminPolicy)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBySearch([FromQuery] int pageSize, 
                                                 [FromQuery] int pageIndex, 
                                                 [FromQuery] Guid? specialtyId, 
                                                 [FromQuery] Guid? doctorId,
                                                 [FromQuery] string? dni,
                                                 [FromQuery] DateOnly? date)
    {
        var citas = await _service.GetBySearch(pageSize, pageIndex, specialtyId, doctorId, dni, date);

        return Ok(citas);
    }
}

