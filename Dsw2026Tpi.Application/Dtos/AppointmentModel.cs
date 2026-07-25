using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentModel
{
    public record Request(Guid Id_doctor, Guid Id_Disponibilidad, DayRequest hora, string dni, string reason);

    public record DayRequest(TimeOnly StartTime, TimeOnly EndTime);
    public record Response(Guid Id);
}
