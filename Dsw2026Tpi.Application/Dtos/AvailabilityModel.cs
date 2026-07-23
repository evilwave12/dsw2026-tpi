using System;
using System.Collections.Generic;
using System.Text;
using static Dsw2026Tpi.Application.Dtos.DoctorModel;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AvailabilityModel
    {
        /*ESTO es de availability*/
        public record DayRequest(string Day, string StartTime, string EndTime);

        public record Request(Guid DoctorId, IEnumerable<DayRequest> Days);

        /*ESTO es el response de doctors*/
        public record Response(byte Day_of_the_week, TimeOnly Start_time, TimeOnly End_time);
    }
}
