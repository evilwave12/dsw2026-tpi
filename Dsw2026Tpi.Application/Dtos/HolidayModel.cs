using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record HolidayModel
    {
        public DateOnly Date { get; init; }
        public string Description { get; init; }
    }
}
