using Dsw2026Tpi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dsw2026Tpi.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");

        builder.HasOne(a => a.Patient)
        .WithMany()
        .HasForeignKey(a => a.Patient_Id);

        builder.HasOne(a => a.Slot)
            .WithMany()
            .HasForeignKey(a => a.Slot_Id);
    }
}
