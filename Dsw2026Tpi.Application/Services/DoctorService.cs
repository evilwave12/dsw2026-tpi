using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System.Numerics;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IPersistence _persistence;

    public DoctorService(IPersistence persistence)
    {
        _persistence = persistence;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {

        if (name != null && (name.Length > 100 || name.Length < 3))
        {
            throw new ValidationException();
        }
        else
        {
            var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex, d => string.IsNullOrWhiteSpace(name) ||
                                                       d.Name.Contains(name) && d.IsActive, x => x.Name, nameof(Doctor.Speciality));

            return doctors.Map(d => new DoctorModel.Response(d.Id, d.Name, d.LicenseNumber,
            new DoctorModel.SpecialityDto(d.Speciality?.Id, d.Speciality?.Name)));
        }
    }

    public async Task<Doctor> GetById(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        return doctor;
    }

    public async Task<IEnumerable<AvailabilityModel.Response>> GetAvailabilities(Guid id_doctor)
    {
        var doctor = await _persistence.GetById<Doctor>(id_doctor);

        if (doctor is null) 
        { 
            throw new EntityNotFoundException(nameof(Doctor)); 
        }
        else
        {
            var disponibilidades = await _persistence.GetFiltered<Availability>(a => a.Doctor_Id == id_doctor);

            return disponibilidades.Select(d => new AvailabilityModel.Response(d.Day_of_the_week,d.Start_time,d.End_time));
        }
    }
}
