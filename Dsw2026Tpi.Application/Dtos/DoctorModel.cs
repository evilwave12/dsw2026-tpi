namespace Dsw2026Tpi.Application.Dtos;

public record DoctorModel
{
    public record Request(string Name, string LicenseNumber, Guid SpecialtyId);
    public record Response(Guid Id, string Name, string LicenseNumber, SpecialtyDto? Specialty);
    public record ResponseAvailability(Guid id, string day,TimeOnly starTime, TimeOnly endTime);
    public record SpecialtyDto(Guid? SpecialtyId, string? Name);

}
