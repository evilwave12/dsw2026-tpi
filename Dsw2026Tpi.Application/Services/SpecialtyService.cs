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

                var specialties = await _persistence.Paginate<Specialty, string>(pageSize, pageIndex, s => (string.IsNullOrWhiteSpace(name) ||
                                                       s.Name.Contains(name)) && !s.Deleted, x => x.Name);

                return specialties.Map(s => new SpecialtyModel.Response(s.Id, s.Name, s.Description));
        }

        public async Task<Specialty> Add(SpecialtyModel.Request specialty) //finikited
        {
            if (string.IsNullOrWhiteSpace(specialty.Name)) throw new ValidationException(ErrorCodes.EMPTY_NAME_ERROR, nameof(ErrorCodes.EMPTY_NAME_ERROR));

            if (specialty.Name.Length > 101 || specialty.Name.Length < 3) throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));  
            
            var especialidades = await _persistence.First<Specialty>(s => s.Name == specialty.Name);

            if (especialidades != null) throw new ConflictException(ErrorCodes.SPECIALTY_EXISTS, nameof(ErrorCodes.SPECIALTY_EXISTS)).WithDetail("name", "specialty_already_exists");
                   
            if (string.IsNullOrWhiteSpace(specialty.Description)) throw new ValidationException(ErrorCodes.EMPTY_DESCRIPTION_ERROR, nameof(ErrorCodes.EMPTY_DESCRIPTION_ERROR));

            if (specialty.Description.Length > 101 || specialty.Description.Length < 11) throw new ValidationException(ErrorCodes.DESCRIPTION_ERROR, nameof(ErrorCodes.DESCRIPTION_ERROR));

            _logger.LogInformation($"Especialidad {specialty.Name} creada exitosamente con descripción {specialty.Description}");

            return await _persistence.Add(new Specialty(specialty.Name, specialty.Description));
            
        }

        public async Task Delete(Guid id) //finikited
        {
            var specialty = await _persistence.GetById<Specialty>(id) ?? throw new EntityNotFoundException(nameof(Specialty));

            if (specialty.Deleted)
            {
                throw new ConflictException(ErrorCodes.SPECIALTY_INACTIVE, nameof(ErrorCodes.SPECIALTY_INACTIVE));
            }

            specialty.Deactivate();

            await _persistence.Update(specialty);

            _logger.LogInformation($"Especialidad {specialty.Name} eliminada exitosamente");
        }

        public async Task <Specialty> Update (Guid id, SpecialtyModel.Request specialty) //finikited
        {
            var specialty2 = await _persistence.GetById<Specialty>(id) ?? throw new EntityNotFoundException(nameof(Specialty));

            if (specialty2.Deleted) throw new ConflictException(ErrorCodes.SPECIALTY_INACTIVE, nameof(ErrorCodes.SPECIALTY_INACTIVE)).WithDetail("specialtyDeleted", "specialty_already_deleted");

            if (string.IsNullOrWhiteSpace(specialty.Name)) throw new ValidationException(ErrorCodes.EMPTY_NAME_ERROR, nameof(ErrorCodes.EMPTY_NAME_ERROR));

            if (specialty.Name.Length > 101 || specialty.Name.Length < 3) throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));

            if (string.IsNullOrWhiteSpace(specialty.Description)) throw new ValidationException(ErrorCodes.EMPTY_DESCRIPTION_ERROR, nameof(ErrorCodes.EMPTY_DESCRIPTION_ERROR));

            if (specialty.Description.Length > 101 || specialty.Description.Length < 11) throw new ValidationException(ErrorCodes.DESCRIPTION_ERROR, nameof(ErrorCodes.DESCRIPTION_ERROR));


            specialty2.Name = specialty.Name;
            specialty2.Description = specialty.Description;

            return await _persistence.Update(specialty2);
           
        }

    }
}
