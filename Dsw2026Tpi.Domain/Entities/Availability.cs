using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public class Availability : EntityBase
    {
        public byte Month { get; init; }
        public short Year { get; init; }
        public byte Day_of_the_week { get; init; }
        public DateTime Start_time { get; init; }
        public DateTime End_time { get; init; }

        #region Constructor for EF
#pragma warning disable CS8618
        private Availability()
        {
        }
#pragma warning restore CS8618
        #endregion

        public Availability(byte month, short year, byte day_of_the_week, DateTime start_time, DateTime end_time) : base()
        {
            Month = month;
            Year = year;
            Day_of_the_week = day_of_the_week;
            Start_time = start_time;
            End_time = end_time;
        }
    }
}
