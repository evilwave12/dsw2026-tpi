using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Appointment : EntityBase
    {
        public string Reason { get; init; }
        public DateTime? CancelledAt { get; init; }
        public DateTime? AttendedAt { get; init; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
        public Guid SlotId { get; set; }
        public Guid PatientId { get; set; }

        public AvailabilitySlot Slot { get; private set; } //propiedad navegacion
        public Patient Patient { get; private set; } //propiedad navegacion

        #region Constructor for EF
#pragma warning disable CS8618
        private Appointment ()
        {
        }
#pragma warning restore CS8618
        #endregion

        public Appointment(string reason, Guid slotId, Guid patientId) : base()
        {
            Reason = reason;
            SlotId = slotId;
            PatientId = patientId;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
    }
}
