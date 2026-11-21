using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Numerics;

namespace Dsw2026Tpi.Application.Services;

public class DoctorService : IDoctorService
{
    private readonly IPersistence _persistence;
    private readonly ILogger<DoctorService> _logger;

    public DoctorService(IPersistence persistence, ILogger<DoctorService> logger)
    {
        _persistence = persistence;
        _logger = logger;
    }

    public async Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
    {
        if (name != null && (name.Length > 100 || name.Length < 3))
        {
            throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));
        }
        else 
        {
            /*var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex, d => (string.IsNullOrWhiteSpace(name) ||
                                                       d.Name.Contains(name)) && d.IsActive, x => x.Name, nameof(Doctor.Specialty));*/

            var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex, d => (string.IsNullOrWhiteSpace(name) ||
                                                       d.Name.Contains(name)) && !d.Deleted, x => x.Name, nameof(Doctor.Specialty));

            return doctors.Map(d => new DoctorModel.Response(d.Id, d.Name, d.LicenseNumber,d.IsActive,
            new DoctorModel.SpecialtyDto(d.Specialty?.Id, d.Specialty?.Name)));
        }
    }

    public async Task<IEnumerable<DoctorModel.ResponseAvailability>> GetAvailabilities(Guid idDoctor)
    {
        var doctor = await _persistence.GetById<Doctor>(idDoctor);
        
        if(doctor is null || doctor.Deleted) throw new EntityNotFoundException(nameof(Doctor));
        
        var availabilities = await _persistence.GetFiltered<Availability>(a => a.DoctorId == idDoctor);
          
        return availabilities.Select(d => new DoctorModel.ResponseAvailability(d.Id, ((DiaSemana)d.DayOfTheWeek).ToString(), d.StartTime, d.EndTime));
    }

    public async Task<Doctor> Add(DoctorModel.Request doctor)
    {
        #region Validaciones
        if (string.IsNullOrWhiteSpace(doctor.name)) throw new ValidationException(ErrorCodes.EMPTY_NAME_ERROR, nameof(ErrorCodes.EMPTY_NAME_ERROR));

        if(doctor.name.Length > 100 || doctor.name.Length < 3) throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));

        if (string.IsNullOrWhiteSpace(doctor.licenseNumber)) throw new ValidationException(ErrorCodes.EMPTY_LICENSE_ERROR, nameof(ErrorCodes.EMPTY_LICENSE_ERROR));
        if (doctor.licenseNumber.Length > 50) throw new ValidationException(ErrorCodes.INVALID_LICENSE_ERROR, nameof(ErrorCodes.INVALID_LICENSE_ERROR));

        var license = await _persistence.GetFiltered<Doctor>(d => d.LicenseNumber == doctor.licenseNumber);

        if (license.Any()) throw new ConflictException(ErrorCodes.DUPLICATE_LICENSE_ERROR, nameof(ErrorCodes.DUPLICATE_LICENSE_ERROR)).WithDetail("licenseNumber", "license_number_already_exists");

        var specialty = await _persistence.GetById<Specialty>(doctor.specialtyId) ?? throw new EntityNotFoundException(nameof(Specialty));
        if (specialty.Deleted) throw new ValidationException(ErrorCodes.INVALID_SPECIALTY_ERROR, nameof(ErrorCodes.INVALID_SPECIALTY_ERROR));
        #endregion

        var newDoctor = await _persistence.Add(new Doctor(doctor.name, doctor.licenseNumber, doctor.specialtyId));

        _logger.LogInformation($"Doctor {newDoctor.Name} creado exitosamente con matrícula {newDoctor.LicenseNumber} y especialidad {specialty.Name}");

        return newDoctor;
    }

    public async Task<Doctor> Update(Guid id, DoctorModel.Request doctorRequest)
    {
        #region Validaciones
        var doctorExistente = await _persistence.GetById<Doctor>(id) ?? throw new EntityNotFoundException(nameof(Doctor));

        if(doctorExistente is null || doctorExistente.Deleted) throw new EntityNotFoundException(nameof(Doctor));

        if (string.IsNullOrWhiteSpace(doctorRequest.name)) throw new ValidationException(ErrorCodes.EMPTY_NAME_ERROR, nameof(ErrorCodes.EMPTY_NAME_ERROR));

        if (doctorRequest.name.Length > 100 || doctorRequest.name.Length < 3) throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));

        if (string.IsNullOrWhiteSpace(doctorRequest.licenseNumber)) throw new ValidationException(ErrorCodes.EMPTY_LICENSE_ERROR, nameof(ErrorCodes.EMPTY_LICENSE_ERROR));
        if (doctorRequest.licenseNumber.Length > 50) throw new ValidationException(ErrorCodes.INVALID_LICENSE_ERROR, nameof(ErrorCodes.INVALID_LICENSE_ERROR));
        #endregion

        doctorExistente.Name = doctorRequest.name;
        doctorExistente.LicenseNumber = doctorRequest.licenseNumber;

        var specialty = await _persistence.GetById<Specialty>(doctorRequest.specialtyId) ?? throw new EntityNotFoundException(nameof(Specialty));
        if (doctorExistente.SpecialtyId != doctorRequest.specialtyId) //si es q se cambia el id de especialidad, actualizar la propia especialidad del medico tambien
        {
            if (specialty.Deleted) throw new ValidationException(ErrorCodes.INVALID_SPECIALTY_ERROR, nameof(ErrorCodes.INVALID_SPECIALTY_ERROR));

            doctorExistente.SpecialtyId = doctorRequest.specialtyId;
        }

        var now = DateTime.Now;
        doctorExistente.UpdatedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        await _persistence.Update(doctorExistente);
        return doctorExistente;
    }

    public async Task Delete(Guid id)
    {
        var doctor = await _persistence.GetById<Doctor>(id) ?? throw new EntityNotFoundException(nameof(Doctor));

        if(doctor.Deleted) throw new ValidationException(ErrorCodes.DOCTOR_INACTIVE, nameof(ErrorCodes.DOCTOR_INACTIVE));

        doctor.Delete();

        var now = DateTime.Now;
        doctor.UpdatedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

        await _persistence.Update(doctor);

        _logger.LogInformation($"Doctor {doctor.Name} con matrícula {doctor.LicenseNumber} ha sido borrado exitosamente.");
    }
}
