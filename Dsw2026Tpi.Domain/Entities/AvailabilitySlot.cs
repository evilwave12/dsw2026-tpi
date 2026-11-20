using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class AvailabilitySlot : EntityBase
    {
        public DateOnly SlotDate { get; init; }
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }
        public bool IsActive { get; private set; } = false;
        public AvailabilitySlotStatus Status { get; set; } = AvailabilitySlotStatus.Available;
        public Guid AvailabilityId { get; set; }
        public Availability Availability { get; set; } //prop navegacion


        #region Constructor for EF
#pragma warning disable CS8618
        private AvailabilitySlot()
        {
        }
#pragma warning restore CS8618
        #endregion

        public AvailabilitySlot(Guid availabilityId, DateOnly slotDate, TimeOnly startTime, TimeOnly endTime) : base()
        {
            AvailabilityId = availabilityId;
            SlotDate = slotDate;
            StartTime = startTime;
            EndTime = endTime;
        }
        public void Deactivate()
        {
            IsActive = true;
        }
    }
}
