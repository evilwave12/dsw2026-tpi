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

    public async Task<Pagination<DoctorModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null) //finikited
    {
        if (name != null && (name.Length > 100 || name.Length < 3))
        {
            throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));
        }
        else 
        {
            var doctors = await _persistence.Paginate<Doctor, string>(pageSize, pageIndex, d => (string.IsNullOrWhiteSpace(name) ||
                                                       d.Name.Contains(name)) && d.IsActive, x => x.Name, nameof(Doctor.Specialty));

            return doctors.Map(d => new DoctorModel.Response(d.Id, d.Name, d.LicenseNumber,
            new DoctorModel.SpecialtyDto(d.Specialty?.Id, d.Specialty?.Name, d.Specialty.Description)));
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

        var specialty = await _persistence.GetById<Specialty>(doctor.SpecialtyId) ?? throw new EntityNotFoundException(nameof(Specialty));
        if (specialty.Deleted) throw new ValidationException(ErrorCodes.INVALID_SPECIALTY_ERROR, nameof(ErrorCodes.INVALID_SPECIALTY_ERROR));

        var newDoctor = await _persistence.Add(new Doctor(doctor.Name, doctor.LicenseNumber, doctor.SpecialtyId));

        _logger.LogInformation($"Doctor {newDoctor.Name} creado exitosamente con matrícula {newDoctor.LicenseNumber} y especialidad {specialty.Name}");

        return new DoctorModel.Response(newDoctor.Id, newDoctor.Name, newDoctor.LicenseNumber, new DoctorModel.SpecialtyDto(specialty.Id, specialty.Name, specialty.Description));
    }

    public async Task<DoctorModel.Response> Update(Guid id, DoctorModel.Request doctorRequest)
    {
        var doctorExistente = await _persistence.GetById<Doctor>(id) ?? throw new EntityNotFoundException(nameof(Doctor));

        if (string.IsNullOrWhiteSpace(doctorRequest.Name)) throw new ValidationException(ErrorCodes.EMPTY_NAME_ERROR, nameof(ErrorCodes.EMPTY_NAME_ERROR));

        if (doctorRequest.Name.Length > 101 || doctorRequest.Name.Length < 3) throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));

        if (string.IsNullOrWhiteSpace(doctorRequest.LicenseNumber)) throw new ValidationException(ErrorCodes.LICENSE_ERROR, nameof(ErrorCodes.LICENSE_ERROR));


        doctorExistente.Name = doctorRequest.Name;
        doctorExistente.LicenseNumber = doctorRequest.LicenseNumber;

        var specialty = await _persistence.GetById<Specialty>(doctorRequest.SpecialtyId) ?? throw new EntityNotFoundException(nameof(Specialty));
        if (doctorExistente.SpecialtyId != doctorRequest.SpecialtyId) //si es q se cambia el id de especialidad, actualizar la propia especialidad del medico tambien
        {
            
            if (specialty.Deleted) throw new ValidationException(ErrorCodes.INVALID_SPECIALTY_ERROR, nameof(ErrorCodes.INVALID_SPECIALTY_ERROR));

            doctorExistente.SpecialtyId = doctorRequest.SpecialtyId;

            await _persistence.Update(doctorExistente);
            return new DoctorModel.Response(doctorExistente.Id, doctorExistente.Name, doctorExistente.LicenseNumber, new DoctorModel.SpecialtyDto(doctorExistente.Specialty.Id, doctorExistente.Specialty.Name, doctorExistente.Specialty.Description));
        }

        await _persistence.Update(doctorExistente);

        return new DoctorModel.Response(doctorExistente.Id, doctorExistente.Name, doctorExistente.LicenseNumber, new DoctorModel.SpecialtyDto(specialty.Id, specialty.Name, specialty.Description));
    }

    public async Task Delete(Guid id) //finikited
    {
        var doctor = await _persistence.GetById<Doctor>(id) ?? throw new EntityNotFoundException(nameof(Doctor));

        if(!doctor.IsActive) throw new ValidationException(ErrorCodes.DOCTOR_INACTIVE, nameof(ErrorCodes.DOCTOR_INACTIVE));

        doctor.Deactivate();

        await _persistence.Update(doctor);

        _logger.LogInformation($"Doctor {doctor.Name} con matrícula {doctor.LicenseNumber} ha sido borrado exitosamente.");
    }
}
