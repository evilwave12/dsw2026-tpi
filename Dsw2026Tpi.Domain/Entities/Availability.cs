using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Availability : EntityBase
    {
        public byte Month { get; init; }
        public short Year { get; init; }
        public byte DayOfTheWeek { get; init; }
        public TimeOnly StartTime { get; init; }
        public TimeOnly EndTime { get; init; }
        public Guid DoctorId { get; set; }
        public Doctor Doctor { get; private set; } //propiedad navegacion

        #region Constructor for EF
#pragma warning disable CS8618
        private Availability()
        {
        }
#pragma warning restore CS8618
        #endregion

        public Availability(Guid doctorId, byte month, short year, byte dayOfTheWeek, TimeOnly startTime, TimeOnly endTime) : base()
        {
            DoctorId = doctorId;
            Month = month;
            Year = year;
            DayOfTheWeek = dayOfTheWeek;
            StartTime = startTime;
            EndTime = endTime;
        }

        [JsonConstructor]
        public Availability(Guid doctorId, byte dayOfTheWeek, TimeOnly startTime, TimeOnly endTime) : base()
        {
            DoctorId = doctorId;
            DayOfTheWeek = dayOfTheWeek;
            StartTime = startTime;
            EndTime = endTime;

            Month = (byte)DateTime.Now.Month;
            Year = (short)DateTime.Now.Year;
        }
    }
}
