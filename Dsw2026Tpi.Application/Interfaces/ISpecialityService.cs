using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface ISpecialityService
    {
        Task<Pagination<SpecialityModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null);
        Task<Speciality> Delete(Guid id);
    }
}

