using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Dtos
{
    public record SpecialtyModel
    {
        public record Request(string name, string description, bool isActive);
        public record Response(Guid id, string name, string description, bool isActive);
    }
}
