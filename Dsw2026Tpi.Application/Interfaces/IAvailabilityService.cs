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
        Task<IEnumerable<AvailabilityModel.Response>> CreateAvailabilitiesAsync(AvailabilityModel.Request request);
        Task ValidationAvailabilitiesAsync(AvailabilityModel.Request request);
        Task<IEnumerable<AvailabilityModel.Response>> Update(AvailabilityModel.Request request);
    }
}
