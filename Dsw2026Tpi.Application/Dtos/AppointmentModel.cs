using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentModel
{
    public record Request(Guid Id_doctor, Guid Id_Slot, string dni, string reason);
    public record Response(Guid Id_Doctor,string Doctor_Name, string Dni_Pac, string Nombre_Pac,TimeOnly StartTime, TimeOnly EndTime); //admin endpoint 1
    public record ResponseGetBySearch(string specialityName, string doctorName,DateOnly fecha, TimeOnly inicio, TimeOnly fin); //admin endpoint 2
}
