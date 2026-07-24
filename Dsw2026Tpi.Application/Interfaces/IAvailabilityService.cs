using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Data.Migrations;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface IAvailabilityService
    {
        Task CreateAvailabilitiesAsync(AvailabilityModel.Request request);
        Task <Availability>UpdateAvailabilitiesAsync(AvailabilityModel.Request request);
    }
}
