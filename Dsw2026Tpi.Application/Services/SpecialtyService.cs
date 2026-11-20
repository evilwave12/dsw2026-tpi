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
                                                       s.Name.Contains(name)), x => x.Name);

            return specialties.Map(s => new SpecialtyModel.Response(s.Id, s.Name, s.Description,s.IsActive));
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

            return await _persistence.Add(new Specialty(specialty.name, specialty.description, specialty.isActive));
            
        }

        public async Task Delete(Guid id)
        {
            var specialty = await _persistence.GetById<Specialty>(id) ?? throw new EntityNotFoundException(nameof(Specialty));

            if (!specialty.IsActive)
            {
                throw new ConflictException(ErrorCodes.SPECIALTY_INACTIVE, nameof(ErrorCodes.SPECIALTY_INACTIVE));
            }

            specialty.Deactivate();

            var now = DateTime.Now;
            specialty.UpdatedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

            await _persistence.Update(specialty);

            _logger.LogInformation($"Especialidad {specialty.Name} eliminada exitosamente");
        }

        public async Task <Specialty> Update(Guid id, SpecialtyModel.Request specialty)
        {
            #region Validaciones
            var specialty2 = await _persistence.GetById<Specialty>(id) ?? throw new EntityNotFoundException(nameof(Specialty));

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
