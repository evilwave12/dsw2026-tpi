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
        public byte Day_of_the_week { get; init; }
        public TimeOnly Start_time { get; init; }
        public TimeOnly End_time { get; init; }
        public Guid Doctor_Id { get; set; }
        public Doctor Doctor { get; private set; } //propiedad navegacion

        #region Constructor for EF
#pragma warning disable CS8618
        private Availability()
        {
        }
#pragma warning restore CS8618
        #endregion

        public Availability(Guid doctorId, byte month, short year, byte day_of_the_week, TimeOnly start_time, TimeOnly end_time) : base()
        {
            Doctor_Id = doctorId;
            Month = month;
            Year = year;
            Day_of_the_week = day_of_the_week;
            Start_time = start_time;
            End_time = end_time;
        }

        [JsonConstructor]
        public Availability(Guid doctor_Id, byte day_of_the_week, TimeOnly start_time, TimeOnly end_time) : base()
        {
            Doctor_Id = doctor_Id;
            Day_of_the_week = day_of_the_week;
            Start_time = start_time;
            End_time = end_time;

            Month = (byte)DateTime.Now.Month;
            Year = (short)DateTime.Now.Year;
        }
    }
}
