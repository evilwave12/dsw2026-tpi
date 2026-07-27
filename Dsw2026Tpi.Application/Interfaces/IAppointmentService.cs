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
        Task<IEnumerable<Appointment>> GetActiveAppointmentsByPatientDni(string dni);
        Task<Appointment> Add(AppointmentModel.Request cita);
        Task CancelAppointment(Guid id);
        Task<List<AppointmentModel.ResponseGetByDate>> GetByDate(DateOnly date);
        Task<Pagination<AppointmentModel.ResponseGetBySearch>> GetBySearch(int pageSize, int pageIndex, Guid? specialityId = null, Guid? doctorId = null, string? dni = null, DateOnly? date = null);

    }
}