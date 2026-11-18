using System;
using System.Collections.Generic;
using System.Text;
using static Dsw2026Tpi.Application.Dtos.DoctorModel;

namespace Dsw2026Tpi.Application.Dtos
{
    public record AvailabilityModel
    {
        public record Response(string dayOfTheWeek, string startTime, string endTime);
        public record DayRequest(string dayOfTheWeek, TimeOnly startTime, TimeOnly endTime);
        public record Request(Guid DoctorId, IEnumerable<DayRequest> Days);
    }

    public enum DiaSemana : byte
    {
        Domingo = 0,
        Lunes = 1,
        Martes = 2,
        Miercoles = 3,
        Jueves = 4,
        Viernes = 5,
        Sábado = 6
    }
}
