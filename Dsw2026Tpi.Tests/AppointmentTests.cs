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
    public class AppointmentTests
    {
        private readonly IPersistence _mockPersistence;
        private readonly AppointmentService _service;
        private readonly ILogger<AppointmentService> _mockLogger;

        public AppointmentTests()
        {
            _mockPersistence = Substitute.For<IPersistence>();
            _mockLogger = Substitute.For<ILogger<AppointmentService>>();
            _service = new AppointmentService(_mockPersistence, _mockLogger);

        }

        [Fact]
        public async Task AgregarTurno_CuandoElSlotYaEstaReservado_EntoncesSeLanzaUnaExcepcion()
        {
            //Arrange

            var doctorId = Guid.NewGuid();
            var slotId = Guid.NewGuid();

            var request = new AppointmentModel.Request
            (
                Id_doctor: doctorId,
                Id_Slot: slotId,
                dni: "45513019",
                reason: "Consulta general"
            );

            var newDoctor = new Doctor("Dr. Perez", "MAT-9080", Guid.NewGuid());
            var newSlot = new AvailabilitySlot(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                                         new TimeOnly(10, 0), new TimeOnly(10, 30))
            {
                Status = AvailabilitySlotStatus.Booked
            };
            var newPatient = new Patient("Ian Fernandez Campos", "45513019", Guid.NewGuid());

            _mockPersistence.GetById<Doctor>(doctorId).Returns(newDoctor);
            _mockPersistence.GetById<AvailabilitySlot>(slotId).Returns(newSlot);
            _mockPersistence.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>()).Returns(newPatient);

            //Act

            var exception = await Assert.ThrowsAsync<ConflictException>(() => _service.Add(request));

            //Assert

            Assert.Equal(nameof(ErrorCodes.SLOT_NOT_AVAILABLE_CONFLICT),exception.Error.ErrorCode);

        }

        [Fact]
        public async Task AgregarTurno_CuandoElSlotEstaDisponible_EntoncesSeCreaEnLaBaseDeDatos()
        {
            //Arrange
            var doctorId = Guid.NewGuid();
            var slotId = Guid.NewGuid();

            var request = new AppointmentModel.Request
            (
                Id_doctor: doctorId,
                Id_Slot: slotId,
                dni: "45513019",
                reason: "Consulta general"
            );

            var expectedResponse = new AppointmentModel.Response
            (
                Id_Doctor: doctorId,
                Doctor_Name: "Dr. Perez",
                Dni_Pac: "45513019",
                Nombre_Pac: "Ian Nazareno Fernandez Campos",
                StartTime: new TimeOnly(10, 0),
                EndTime: new TimeOnly(10, 30)
            );

            var newDoctor = new Doctor("Dr. Perez", "MAT-9080",Guid.NewGuid(),doctorId);
            var newSlot = new AvailabilitySlot(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now.AddDays(1)),
                                         new TimeOnly(10, 0), new TimeOnly(10, 30))
            {
                Status = AvailabilitySlotStatus.Available
            };

            var newPatient = new Patient("Ian Nazareno Fernandez Campos", "45513019", Guid.NewGuid());

            _mockPersistence.GetById<Doctor>(doctorId).Returns(newDoctor);
            _mockPersistence.GetById<AvailabilitySlot>(slotId).Returns(newSlot);
            _mockPersistence.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>()).Returns(newPatient);

            //Act
            var result = await _service.Add(request);

            //Assert

            var created = result.Should().BeOfType<AppointmentModel.Response>().Subject;
            created.Should().Be(expectedResponse);

            await _mockPersistence.Received(1).Add(Arg.Any<Appointment>());
            await _mockPersistence.Received(1).Update(Arg.Any<AvailabilitySlot>());
            
        }

        [Fact]
        public async Task AgregarTurno_CuandoLaFechaEsPasada_EntoncesSeLanzaUnaExcepcion()
        {
            //Arrange
            var doctorId = Guid.NewGuid();
            var slotId = Guid.NewGuid();
            var request = new AppointmentModel.Request
            (
                Id_doctor: doctorId,
                Id_Slot: slotId,
                dni: "45513019",
                reason: "Consulta general"
            );

            var newPatient = new Patient("Ian Fernandez Campos", "45513019", Guid.NewGuid());
            var newDoctor = new Doctor("Dr. Perez", "MAT-9080", Guid.NewGuid());
            var newSlot = new AvailabilitySlot(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                                         new TimeOnly(10, 0), new TimeOnly(10, 30));
           
            _mockPersistence.GetById<Doctor>(doctorId).Returns(newDoctor);
            _mockPersistence.GetById<AvailabilitySlot>(slotId).Returns(newSlot);
            _mockPersistence.First<Patient>(Arg.Any<Expression<Func<Patient, bool>>>()).Returns(newPatient);

            //Act

            var exception = await Assert.ThrowsAsync<ConflictException>(() => _service.Add(request));

            //Assert

            Assert.Equal(nameof(ErrorCodes.PAST_DATE_CONFLICT), exception.Error.ErrorCode);
        }

    }
}
