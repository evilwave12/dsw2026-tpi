using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Domain.Entities;

namespace Dsw2026Tpi.Application.Interfaces;

public interface IDoctorService
{
    Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null);
    Task<IEnumerable<AvailabilityModel.Response>> GetAvailabilities(Guid id_doctor);
    Task<Doctor> Add(DoctorModel.Request doctor);
    Task<Doctor> Update(Guid id, DoctorModel.Request doctor);
}
