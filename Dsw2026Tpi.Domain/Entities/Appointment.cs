using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Appointment : EntityBase
    {
        public string Reason { get; init; }
        public DateTime? Cancelled_at { get; init; }
        public DateTime? Attended_at { get; init; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
        public Guid Slot_Id { get; set; }
        public Guid Patient_Id { get; set; }

        public AvailabilitySlot Slot { get; private set; } //propiedad navegacion
        public Patient Patient { get; private set; } //propiedad navegacion

        #region Constructor for EF
#pragma warning disable CS8618
        private Appointment ()
        {
        }
#pragma warning restore CS8618
        #endregion

        public Appointment(string reason, Guid Slot_id, Guid Patient_id) : base()
        {
            Reason = reason;
            Slot_Id = Slot_id;
            Patient_Id = Patient_id;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
    }
}
