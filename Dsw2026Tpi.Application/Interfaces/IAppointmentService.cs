using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentModel.Response> Add(AppointmentModel.Request cita);
        Task<IEnumerable<AppointmentModel.Response>> GetActiveAppointmentsByPatientDni(string dni);
        Task CancelAppointment(Guid id);
        Task<List<AppointmentModel.Response>> GetByDate(DateOnly date);
        Task<Pagination<AppointmentModel.ResponseGetBySearch>> GetBySearch(int pageSize, int pageIndex, Guid? specialtyId = null, Guid? doctorId = null, string? dni = null, DateOnly? date = null);

    }
}