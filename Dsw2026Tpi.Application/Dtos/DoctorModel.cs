namespace Dsw2026Tpi.Application.Dtos;

public record DoctorModel
{
    public record Request(string name, string licenseNumber, Guid specialtyId);
    public record Response(Guid id, string name, string licenseNumber, bool isActive, SpecialtyDto? specialty);
    public record ResponseAvailability(Guid id, string day,TimeOnly startTime, TimeOnly endTime);
    public record SpecialtyDto(Guid? specialtyId, string? name);

}
