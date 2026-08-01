using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Dsw2026Tpi.Application.Services
{
    public class SpecialityService : ISpecialityService
    {
        private readonly IPersistence _persistence;

        public SpecialityService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task<Pagination<SpecialityModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null) //finikited
        {
            if (name != null && (name.Length > 100 || name.Length < 3))
            { 
                throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));
            }

                var specialities = await _persistence.Paginate<Speciality, string>(pageSize, pageIndex, s => (string.IsNullOrWhiteSpace(name) ||
                                                       s.Name.Contains(name)) && !s.Deleted, x => x.Name);

                return specialities.Map(s => new SpecialityModel.Response(s.Id, s.Name, s.Description));
        }

        public async Task<Speciality> Add(SpecialityModel.Request speciality) //finikited
        {
            if (string.IsNullOrWhiteSpace(speciality.Name)) throw new ValidationException(ErrorCodes.EMPTY_NAME_ERROR, nameof(ErrorCodes.EMPTY_NAME_ERROR));

            if (speciality.Name.Length > 101 || speciality.Name.Length < 3) throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));  
            
            var especialidades = await _persistence.First<Speciality>(s => s.Name == speciality.Name);

            if (especialidades != null) throw new ConflictException("La especialidad ya existe", "SPECIALITY_EXISTS").WithDetail("Speciality_Name", "Speciality_Already_Exists");
                   
            if (string.IsNullOrWhiteSpace(speciality.Description)) throw new ValidationException(ErrorCodes.EMPTY_DESCRIPTION_ERROR, nameof(ErrorCodes.EMPTY_DESCRIPTION_ERROR));

            if (speciality.Description.Length > 101 || speciality.Description.Length < 11) throw new ValidationException(ErrorCodes.DESCRIPTION_ERROR, nameof(ErrorCodes.DESCRIPTION_ERROR));

            return await _persistence.Add(new Speciality(speciality.Name, speciality.Description));
            
        }

        public async Task Delete(Guid id) //finikited
        {
            var speciality = await _persistence.GetById<Speciality>(id) ?? throw new EntityNotFoundException(nameof(Speciality));

            if (speciality.Deleted)
            {
                throw new ConflictException("La especialidad ya está eliminada", "SPECIALITY_INACTIVE");
            }

            speciality.Deactivate();

            await _persistence.Update(speciality);
        }

        public async Task <Speciality> Update (Guid id, SpecialityModel.Request speciality) //finikited
        {
            var speciality2 = await _persistence.GetById<Speciality>(id) ?? throw new EntityNotFoundException(nameof(Speciality));

            if (speciality2.Deleted) throw new ConflictException("La especialidad está eliminada", "SPECIALITY_INACTIVE").WithDetail("Speciality_Deleted", "Speciality_Already_Deleted");

            if (string.IsNullOrWhiteSpace(speciality.Name)) throw new ValidationException(ErrorCodes.EMPTY_NAME_ERROR, nameof(ErrorCodes.EMPTY_NAME_ERROR));

            if (speciality.Name.Length > 101 || speciality.Name.Length < 3) throw new ValidationException(ErrorCodes.NAME_ERROR, nameof(ErrorCodes.NAME_ERROR));

            if (string.IsNullOrWhiteSpace(speciality.Description)) throw new ValidationException(ErrorCodes.EMPTY_DESCRIPTION_ERROR, nameof(ErrorCodes.EMPTY_DESCRIPTION_ERROR));

            if (speciality.Description.Length > 101 || speciality.Description.Length < 11) throw new ValidationException(ErrorCodes.DESCRIPTION_ERROR, nameof(ErrorCodes.DESCRIPTION_ERROR));


            speciality2.Name = speciality.Name;
            speciality2.Description = speciality.Description;

            return await _persistence.Update(speciality2);
           
        }

    }
}
