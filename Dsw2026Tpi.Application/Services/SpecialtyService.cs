using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Dsw2026Tpi.Application.Services
{
    public class SpecialtyService : ISpecialtyService
    {
        private readonly IPersistence _persistence;
        private readonly ILogger<SpecialtyService> _logger;

        public SpecialtyService(IPersistence persistence, ILogger<SpecialtyService> logger)
        {
            _persistence = persistence;
            _logger = logger;
        }

        public async Task<Pagination<SpecialtyModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null) //finikited
        {
            if (name != null && (name.Length > 100 || name.Length < 3))
            { 
                throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));
            }

            /*var specialties = await _persistence.Paginate<Specialty, string>(pageSize, pageIndex, s => (string.IsNullOrWhiteSpace(name) ||
                                                       s.Name.Contains(name)) && !s.IsActive, x => x.Name);*/

            var specialties = await _persistence.Paginate<Specialty, string>(pageSize, pageIndex, s => (string.IsNullOrWhiteSpace(name) ||
                                                       s.Name.Contains(name)) && !s.Deleted, x => x.Name);

            return specialties.Map(s => new SpecialtyModel.Response(s.Id, s.Name, s.Description));
        }

        public async Task<IEnumerable<SpecialtyModel.ResponseWithDoctors>> GetWithDoctors()
        {
            var specialties = await _persistence.GetAll<Specialty>();

            var specialtiesWithDoctors = new List<SpecialtyModel.ResponseWithDoctors>(); //ir guardando las availabilities creadas para devolverlas al final

            foreach (var spec in specialties)
            {
                var doctors = await _persistence.GetFiltered<Doctor>(d => d.SpecialtyId == spec.Id && !d.Deleted);

                var doctorResponses = doctors.Select(d => new DoctorModel.Response(d.Id, d.Name,d.LicenseNumber,d.IsActive,null)).ToList();

                specialtiesWithDoctors.Add(new SpecialtyModel.ResponseWithDoctors(spec.Id, spec.Name, spec.Description, doctorResponses));
            }
            
            return specialtiesWithDoctors;
        }
       

        public async Task<Specialty> Add(SpecialtyModel.Request specialty)
        {
            #region Validaciones
            if (string.IsNullOrWhiteSpace(specialty.name)) throw new ValidationException(ErrorCodes.EMPTY_NAME_ERROR, nameof(ErrorCodes.EMPTY_NAME_ERROR));

            if (specialty.name.Length > 100 || specialty.name.Length < 3) throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));  
            
            var especialidades = await _persistence.First<Specialty>(s => s.Name == specialty.name);

            if (especialidades != null) throw new ConflictException(ErrorCodes.SPECIALTY_EXISTS, nameof(ErrorCodes.SPECIALTY_EXISTS)).WithDetail("name", "specialty_already_exists");
                   
            if (string.IsNullOrWhiteSpace(specialty.description)) throw new ValidationException(ErrorCodes.EMPTY_DESCRIPTION_ERROR, nameof(ErrorCodes.EMPTY_DESCRIPTION_ERROR));

            if (specialty.description.Length > 100 || specialty.description.Length < 10) throw new ValidationException(ErrorCodes.DESCRIPTION_ERROR, nameof(ErrorCodes.DESCRIPTION_ERROR));
            #endregion

            _logger.LogInformation($"Especialidad {specialty.name} creada exitosamente con descripción {specialty.description}");

            return await _persistence.Add(new Specialty(specialty.name, specialty.description));
            
        }

        public async Task Delete(Guid id)
        {
            var specialty = await _persistence.GetById<Specialty>(id);

            if (specialty is null || specialty.Deleted)
            {
                throw new EntityNotFoundException(nameof(Specialty));
            }

            specialty.Delete();

            var now = DateTime.Now;
            specialty.UpdatedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            await _persistence.Update(specialty);

            _logger.LogInformation($"Especialidad {specialty.Name} eliminada exitosamente");
        }

        public async Task <Specialty> Update(Guid id, SpecialtyModel.Request specialty)
        {
            #region Validaciones
            var specialty2 = await _persistence.GetById<Specialty>(id); 

            if (specialty2 is null || specialty2.Deleted)
            {
                throw new EntityNotFoundException(nameof(Specialty));
            }

            //if (!specialty2.IsActive) throw new ConflictException(ErrorCodes.SPECIALTY_INACTIVE, nameof(ErrorCodes.SPECIALTY_INACTIVE)).WithDetail("specialtyDeleted", "specialty_already_deleted");

            if (string.IsNullOrWhiteSpace(specialty.name)) throw new ValidationException(ErrorCodes.EMPTY_NAME_ERROR, nameof(ErrorCodes.EMPTY_NAME_ERROR));

            if (specialty.name.Length > 101 || specialty.name.Length < 3) throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));

            if (string.IsNullOrWhiteSpace(specialty.description)) throw new ValidationException(ErrorCodes.EMPTY_DESCRIPTION_ERROR, nameof(ErrorCodes.EMPTY_DESCRIPTION_ERROR));

            if (specialty.description.Length > 101 || specialty.description.Length < 11) throw new ValidationException(ErrorCodes.DESCRIPTION_ERROR, nameof(ErrorCodes.DESCRIPTION_ERROR));
            #endregion

            specialty2.Name = specialty.name;
            specialty2.Description = specialty.description;

            var now = DateTime.Now;
            specialty2.UpdatedAt = new DateTime(now.Year,now.Month,now.Day,now.Hour,now.Minute,now.Second);

            return await _persistence.Update(specialty2);
           
        }

    }
}
