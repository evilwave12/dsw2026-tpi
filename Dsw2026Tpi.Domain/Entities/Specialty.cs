using System.Data;
using System.Text.Json.Serialization;

namespace Dsw2026Tpi.Domain.Entities;

public class Specialty: EntityBase
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsActive { get; private set; } = true;

    #region Constructor for EF
#pragma warning disable CS8618
    private Specialty() { }
#pragma warning restore CS8618
    #endregion

    public Specialty(string name, string description, bool isActive, Guid? id = null) : base(id)
    {
        Name = name;
        Description = description;
        IsActive = isActive;
    }

    [JsonConstructor]
    public Specialty(Guid id, string name, string description) : base(id)
    {
        Name = name;
        Description = description;
    }

    public void Deactivate()
    {
        IsActive = false;
    }
}
