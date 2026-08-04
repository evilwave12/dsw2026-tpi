using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Dtos;

public record AppointmentModel
{
    public record PatientDto(string dni);
    public record Request(Guid doctorId, Guid availabilitySlotId, PatientDto patient, string reason);
    public record Response(Guid doctorId, string doctorName, string patientDni, string patientName,TimeOnly startTime, TimeOnly endTime); //admin endpoint 1


    //admin endpoint 2
    public record PatientResponse(string dni, string fullName);
    
    public record SpecialtyResponse(Guid specialtyId, string specialtyName);

    public record DoctorResponse(Guid doctorId, string name, SpecialtyResponse specialty);
    public record ResponseGetBySearch(Guid appointmentsId, AppointmentStatus appointmentsStatus, 
                                        PatientResponse patient,
                                        DoctorResponse doctor);     
}
