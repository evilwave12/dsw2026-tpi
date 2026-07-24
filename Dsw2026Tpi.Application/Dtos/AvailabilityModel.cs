using System;
using System.Collections.Generic;
using System.Text;
using static Dsw2026Tpi.Application.Dtos.DoctorModel;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AvailabilityModel
    {
        public record Response(string day_of_the_week, string start_time, string end_time);
        public record DayRequest(string Day, TimeOnly StartTime, TimeOnly EndTime);
        public record Request(Guid DoctorId, IEnumerable<DayRequest> Days);
    }

    public enum DiaSemana : byte
    {
        Lunes = 1,
        Martes = 2,
        Miércoles = 3,
        Jueves = 4,
        Viernes = 5,
        Sábado = 6,
        Domingo = 7
    }
}
