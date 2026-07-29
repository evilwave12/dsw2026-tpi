using System.Text.Json.Serialization;

namespace Dsw2026Tpi.Domain.Entities;

public class Doctor: EntityBase
{
    public string Name { get; set; }
    public string LicenseNumber { get; set; }
    public bool IsActive { get; private set; }
    public Guid? SpecialityId { get; set; }
    public Speciality? Speciality { get; set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Doctor()
    {
    }
#pragma warning restore CS8618
    #endregion

    public Doctor(string name, string licenseNumber, Guid specialityId, Guid? id = null) : base(id)
    {
        Name = name;
        LicenseNumber = licenseNumber;
        SpecialityId = specialityId;
        IsActive = true;
    }

    [JsonConstructor]
    public Doctor(Guid id, string name, string licenseNumber, Guid? specialityId) : base(id)
    {
        Name = name;
        LicenseNumber = licenseNumber;
        SpecialityId = specialityId;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
