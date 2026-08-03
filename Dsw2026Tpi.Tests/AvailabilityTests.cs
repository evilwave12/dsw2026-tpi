using Castle.Core.Logging;
using Dsw2026Tpi.Api.Controllers;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Services;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dsw2026Tpi.Tests
{
    public class AvailabilityTests
    {
        private readonly IPersistence _mockPersistence;
        private readonly AvailabilityService _service;
        private readonly ILogger<AvailabilityService> _mockLogger;

        public AvailabilityTests()
        {
            _mockPersistence = Substitute.For<IPersistence>();
            _mockLogger = Substitute.For<ILogger<AvailabilityService>>();
            _service = new AvailabilityService(_mockPersistence, _mockLogger);

        }

        [Fact]

        public async Task AgregarDisponibilidad_CuandoHayConflictoDeSolapamiento_EntoncesSeLanzaUnaExcepcion()
        {
            //Arrange
            var doctorId = Guid.NewGuid();

            var newDoctor = new Doctor("Dr. Perez", "MAT-9080", Guid.NewGuid(), doctorId);

            var dayRequest = new AvailabilityModel.DayRequest("Lunes", new TimeOnly(07, 0), new TimeOnly(13, 0));

            var request = new AvailabilityModel.Request(doctorId, new List<AvailabilityModel.DayRequest> { dayRequest });

            var existingAvailability = new Availability(doctorId,5,2026,1,new TimeOnly(07, 0), new TimeOnly(13, 0));

            _mockPersistence.GetFiltered<Availability>(Arg.Any<Expression<Func<Availability, bool>>>())
                            .Returns(new List<Availability> { existingAvailability });

            _mockPersistence.GetById<Doctor>(doctorId).Returns(newDoctor);

            //Act

            var exception = await Assert.ThrowsAsync<ConflictException>(() => _service.CreateAvailabilitiesAsync(request));

            //Assert

            Assert.Equal(nameof(ErrorCodes.SOLAPAMIENTO_CONFLICT), exception.Error.ErrorCode);
        }

    }
}
