using System.Text.Json.Serialization;

namespace Dsw2026Tpi.Domain.Entities;

public class Doctor: EntityBase
{
    public string Name { get; set; }
    public string LicenseNumber { get; set; }
    public bool IsActive { get; private set; }
    public Guid? SpecialtyId { get; set; }
    public Specialty? Specialty { get; set; }

    #region Constructor for EF
#pragma warning disable CS8618
    private Doctor()
    {
    }
#pragma warning restore CS8618
    #endregion

    public Doctor(string name, string licenseNumber, Guid specialtyId, Guid? id = null) : base(id)
    {
        Name = name;
        LicenseNumber = licenseNumber;
        SpecialtyId = specialtyId;
        IsActive = true;
    }

    [JsonConstructor]
    public Doctor(Guid id, string name, string licenseNumber, Guid? specialtyId) : base(id)
    {
        Name = name;
        LicenseNumber = licenseNumber;
        SpecialtyId = specialtyId;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
