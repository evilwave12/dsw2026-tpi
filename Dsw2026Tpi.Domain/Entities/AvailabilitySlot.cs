using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class AvailabilitySlot
    {
        public DateOnly Slot_date { get; init; }
        public TimeOnly Start_time { get; init; }
        public TimeOnly End_time { get; init; }
        public bool Deleted { get; private set; } = false;
        public AvailabilitySlotStatus Status { get; set; } = AvailabilitySlotStatus.Available;

        #region Constructor for EF
#pragma warning disable CS8618
        private AvailabilitySlot()
        {
        }
#pragma warning restore CS8618
        #endregion

        public AvailabilitySlot(DateOnly slot_date, TimeOnly start_time, TimeOnly end_time) : base()
        {
            Slot_date = slot_date;
            Start_time = start_time;
            End_time = end_time;
        }
        public void Deactivate()
        {
            Deleted = true;
        }
    }
}
