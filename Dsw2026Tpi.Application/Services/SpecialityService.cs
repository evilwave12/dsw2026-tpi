using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
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

        public async Task<Pagination<SpecialityModel.Response>> GetAll(int pageSize, int pageIndex, string? name = null)
        {
            if (name != null && (name.Length > 100 || name.Length < 3))
            {
                throw new ValidationException();
            }
            else
            {
                var specialities = await _persistence.Paginate<Speciality, string>(pageSize, pageIndex, s => (string.IsNullOrWhiteSpace(name) ||
                                                       s.Name.Contains(name)) && !s.Deleted, x => x.Name);

                return specialities.Map(s => new SpecialityModel.Response(s.Id, s.Name, s.Description));
            }
            
        }

        public async Task<Speciality> Add(SpecialityModel.Request speciality)
        {
            if (string.IsNullOrWhiteSpace(speciality.Name) || (speciality.Name.Length > 101 && speciality.Name.Length < 3))
            {
                throw new ValidationException();
            }

            if (string.IsNullOrWhiteSpace(speciality.Description) || (speciality.Description.Length > 101 && speciality.Description.Length < 11 ))
            {
                throw new ValidationException();
            }

            return await _persistence.Add(new Speciality(speciality.Name, speciality.Description));
            
        }

        public async Task<Speciality> Delete(Guid id)
        {
            var speciality = await _persistence.GetById<Speciality>(id)
                ?? throw new EntityNotFoundException(nameof(Speciality));

            speciality.Deactivate();
            return await _persistence.Update(speciality);
        }

        public async Task <Speciality> Update (Guid id, SpecialityModel.Request speciality)
        {
            if (string.IsNullOrWhiteSpace(speciality.Name) || (speciality.Name.Length > 101 && speciality.Name.Length < 3))
            {
                throw new ValidationException();
            }

            if (string.IsNullOrWhiteSpace(speciality.Description) || (speciality.Description.Length > 101 && speciality.Description.Length < 11))
            {
                throw new ValidationException();
            }

            var speciality2 = await _persistence.GetById<Speciality>(id)
                ?? throw new EntityNotFoundException(nameof(Speciality));

            speciality2.Name = speciality.Name;
            speciality2.Description = speciality.Description;

            return await _persistence.Update(speciality2);
           
        }

    }
}
