using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentModel
{
    public record Request(Guid Id_doctor, Guid Id_Slot, string dni, string reason);
    public record Response(Guid Id_Doctor,string Doctor_Name, string Dni_Pac, string Nombre_Pac,TimeOnly StartTime, TimeOnly EndTime); //admin endpoint 1


    //admin endpoint 2
    public record PatientResponse(string dni, string fullName);
    
    public record SpecialtyResponse(Guid specialtyId, string specialtyName);

    public record DoctorResponse(Guid doctorId, string name, SpecialtyResponse specialty);
    public record ResponseGetBySearch(Guid appointmentsId, AppointmentStatus appointmentsStatus, 
                                        PatientResponse patient,
                                        DoctorResponse doctor);     
}
