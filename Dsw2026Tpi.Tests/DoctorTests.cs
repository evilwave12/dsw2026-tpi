using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens.Experimental;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dsw2026Tpi.Tests
{
    public class DoctorTests
    {
        private readonly IPersistence _mockPersistence;
        private readonly DoctorService _service;
        private readonly ILogger<DoctorService> _mockLogger;

        public DoctorTests()
        {
            _mockPersistence = Substitute.For<IPersistence>();
            _mockLogger = Substitute.For<ILogger<DoctorService>>();
            _service = new DoctorService(_mockPersistence, _mockLogger);

        }

        [Fact]
        public async Task AgregarDoctor_CuandoMatriculaDuplicada_EntoncesSeLanzaExcepcion()
        {
            //Arrange

            var doctorRequest = new DoctorModel.Request("Dr. House", "MAT123", Guid.NewGuid());
            var doctoresExistentes = new List<Doctor> { new Doctor("Dr. Existente", "MAT123", Guid.NewGuid()) };

            _mockPersistence.GetFiltered<Doctor>(Arg.Any<Expression<Func<Doctor, bool>>>()).Returns(doctoresExistentes);

            //Act

            var exception = await Assert.ThrowsAsync<ConflictException>(() => _service.Add(doctorRequest));

            //Assert

            Assert.Equal(nameof(ErrorCodes.DUPLICATE_LICENSE_ERROR), exception.Error.ErrorCode);

        }

        [Fact]
        public async Task ModificarDoctor_CuandoLaEspecialidadEstaEliminada_EntoncesSeLanzaExcepcion()
        {
            //Arrange

            var doctorId = Guid.NewGuid();
            var specialtyId = Guid.NewGuid();

            var doctorRequest = new DoctorModel.Request("Dr. Strange", "MAT456",specialtyId);
            var doctor = new Doctor(doctorId, "Dr. Strange", "MAT456", Guid.NewGuid());
            var specialty = new Specialty("Podología","Especialistas en pies",specialtyId);

            specialty.Deactivate();

            _mockPersistence.GetById<Doctor>(doctorId).Returns(doctor);
            _mockPersistence.GetById<Specialty>(doctorRequest.SpecialtyId).Returns(specialty);

            //Act

            var exception = await Assert.ThrowsAsync<ValidationException>(() => _service.Update(doctorId, doctorRequest));

            //Assert

            Assert.Equal(nameof(ErrorCodes.INVALID_SPECIALTY_ERROR), exception.Error.ErrorCode);
        }
        

    }
}
