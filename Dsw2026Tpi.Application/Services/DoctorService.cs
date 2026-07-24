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
            var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex, d => (string.IsNullOrWhiteSpace(name) ||
                                                       d.Name.Contains(name)) && d.IsActive, x => x.Name, nameof(Doctor.Speciality));

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

            return disponibilidades.Select(d => new AvailabilityModel.Response(((DiaSemana)d.Day_of_the_week).ToString(), d.Start_time.ToString("HH:mm"), d.End_time.ToString("HH:mm")));
        }
    }

    public async Task<Doctor> Add(DoctorModel.Request doctor) //post
    {
        if (string.IsNullOrWhiteSpace(doctor.Name) || (doctor.Name.Length > 101 && doctor.Name.Length < 3))
        {
            throw new ValidationException();
        }

        var speciality = await _persistence.GetById<Speciality>(doctor.SpecialityId)
                ?? throw new EntityNotFoundException(nameof(Speciality));

        return await _persistence.Add(new Doctor(doctor.Name, doctor.LicenseNumber, speciality));

    }

    public async Task<Doctor> Update(Guid id, DoctorModel.Request doctor)
    {
        if (string.IsNullOrWhiteSpace(doctor.Name) || (doctor.Name.Length > 101 && doctor.Name.Length < 3))
        {
            throw new ValidationException();
        }

        var doctor2 = await _persistence.GetById<Doctor>(id)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        doctor2.Name = doctor.Name;
        doctor2.LicenseNumber = doctor.LicenseNumber;

        if(doctor2.SpecialityId != doctor.SpecialityId) //si es q se cambia el id de especialidad, actualizar la propia especialidad del medico tambien
        {
            var speciality = await _persistence.GetById<Speciality>(doctor.SpecialityId)
            ?? throw new EntityNotFoundException(nameof(Speciality));

            doctor2.SpecialityId = doctor.SpecialityId;
        }
        return await _persistence.Update(doctor2);
    }

    public async Task<Doctor> Delete(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id)
            ?? throw new EntityNotFoundException(nameof(Doctor));

        doctor.Deactivate();
        return await _persistence.Update(doctor);
    }
}
