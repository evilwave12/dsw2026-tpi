using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
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

    public async Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null) //finikited
    {
        if (name != null && (name.Length > 100 || name.Length < 3))
        {
            throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));
        }
        else 
        {
            var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex, d => (string.IsNullOrWhiteSpace(name) ||
                                                       d.Name.Contains(name)) && d.IsActive, x => x.Name, nameof(Doctor.Speciality));

            return doctors.Map(d => new DoctorModel.Response(d.Id, d.Name, d.LicenseNumber,
            new DoctorModel.SpecialityDto(d.Speciality?.Id, d.Speciality?.Name, d.Speciality.Description)));
        }
    }

    public async Task<IEnumerable<AvailabilityModel.Response>> GetAvailabilities(Guid id_doctor) //finikited
    {
        var doctor = await _persistence.GetById<Doctor>(id_doctor) ?? throw new EntityNotFoundException(nameof(Doctor));

        var disponibilidades = await _persistence.GetFiltered<Availability>(a => a.Doctor_Id == id_doctor);
          
        return disponibilidades.Select(d => new AvailabilityModel.Response(((DiaSemana)d.Day_of_the_week).ToString(), d.Start_time.ToString("HH:mm"), d.End_time.ToString("HH:mm")));
    }

    public async Task<DoctorModel.Response> Add(DoctorModel.Request doctor)
    {
        if (string.IsNullOrWhiteSpace(doctor.Name)) throw new ValidationException(ErrorCodes.EMPTY_NAME_ERROR, nameof(ErrorCodes.EMPTY_NAME_ERROR));

        if(doctor.Name.Length > 101 || doctor.Name.Length < 3) throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));

        if (string.IsNullOrWhiteSpace(doctor.LicenseNumber)) throw new ValidationException(ErrorCodes.LICENSE_ERROR, nameof(ErrorCodes.LICENSE_ERROR));

        var matricula = await _persistence.GetFiltered<Doctor>(d => d.LicenseNumber == doctor.LicenseNumber);

        if (matricula.Any()) throw new ConflictException(ErrorCodes.DUPLICATE_LICENSE_ERROR, nameof(ErrorCodes.DUPLICATE_LICENSE_ERROR)).WithDetail("licenseNumber", "license_number_already_exists");

        var speciality = await _persistence.GetById<Speciality>(doctor.SpecialityId) ?? throw new EntityNotFoundException(nameof(Speciality));
        if (speciality.Deleted) throw new ValidationException(ErrorCodes.INVALID_SPECIALITY_ERROR, nameof(ErrorCodes.INVALID_SPECIALITY_ERROR));

        var newDoctor = await _persistence.Add(new Doctor(doctor.Name, doctor.LicenseNumber, doctor.SpecialityId));

        return new DoctorModel.Response(newDoctor.Id, newDoctor.Name, newDoctor.LicenseNumber, new DoctorModel.SpecialityDto(speciality.Id, speciality.Name, speciality.Description));
    }

    public async Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request doctor) //finikited
    {
        var doctor2 = await _persistence.GetById<Doctor>(id) ?? throw new EntityNotFoundException(nameof(Doctor));

        if (string.IsNullOrWhiteSpace(doctor.Name)) throw new ValidationException(ErrorCodes.EMPTY_NAME_ERROR, nameof(ErrorCodes.EMPTY_NAME_ERROR));

        if (doctor.Name.Length > 101 || doctor.Name.Length < 3) throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));

        if (string.IsNullOrWhiteSpace(doctor.LicenseNumber)) throw new ValidationException(ErrorCodes.LICENSE_ERROR, nameof(ErrorCodes.LICENSE_ERROR));


        doctor2.Name = doctor.Name;
        doctor2.LicenseNumber = doctor.LicenseNumber;

        if (doctor2.SpecialityId != doctor.SpecialityId) //si es q se cambia el id de especialidad, actualizar la propia especialidad del medico tambien
        {
            var speciality = await _persistence.GetById<Speciality>(doctor.SpecialityId) ?? throw new EntityNotFoundException(nameof(Speciality));
            if (speciality.Deleted) throw new ValidationException(ErrorCodes.INVALID_SPECIALITY_ERROR, nameof(ErrorCodes.INVALID_SPECIALITY_ERROR));

            doctor2.SpecialityId = doctor.SpecialityId;
        }

        await _persistence.Update(doctor2);

        return new DoctorModel.Response(doctor2.Id, doctor2.Name, doctor2.LicenseNumber, new DoctorModel.SpecialityDto(doctor2.Speciality?.Id, doctor2.Speciality?.Name, doctor2.Speciality?.Description));
    }

    public async Task Delete(Guid id) //finikited
    {
        var doctor = await _persistence.GetById<Doctor>(id) ?? throw new EntityNotFoundException(nameof(Doctor));

        if(!doctor.IsActive) throw new ValidationException(ErrorCodes.DOCTOR_INACTIVE, nameof(ErrorCodes.DOCTOR_INACTIVE));

        doctor.Deactivate();
    }
}
