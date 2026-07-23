using System.Data;
using System.Text.Json.Serialization;

namespace Dsw2026Tpi.Domain.Entities;

public class Speciality: EntityBase
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Deleted { get; private set; } = false;

    #region Constructor for EF
#pragma warning disable CS8618
    private Speciality() { }
#pragma warning restore CS8618
    #endregion

    public Speciality(string name, string description, Guid? id = null) : base(id)
    {
        Name = name;
        Description = description;
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
    }

    [JsonConstructor]
    public Speciality(Guid id, string name, string description) : base(id)
    {
        Name = name;
        Description = description;
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
    }

    public void Deactivate()
    {
        Deleted = true;
    }
}
