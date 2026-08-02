using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Interfaces
{
    public interface ISpecialtyService
    {
        Task<Pagination<SpecialtyModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null);
        Task Delete(Guid id);
        Task<Specialty> Add(SpecialtyModel.Request specialty);
        Task <Specialty> Update (Guid id, SpecialtyModel.Request specialty);

    }
}

