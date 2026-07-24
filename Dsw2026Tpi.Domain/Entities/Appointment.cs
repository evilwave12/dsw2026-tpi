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

        #region Constructor for EF
#pragma warning disable CS8618
        private Appointment ()
        {
        }
#pragma warning restore CS8618
        #endregion

        public Appointment(string reason, DateTime? cancelled_at, DateTime? attended_at, Guid slot_Id) : base()
        {
            Reason = reason;
            Cancelled_at = cancelled_at;
            Attended_at = attended_at;
            Slot_Id = slot_Id;
            CreatedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }
    }
}
