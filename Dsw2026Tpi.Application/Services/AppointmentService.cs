using Azure;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IPersistence _persistence;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(IPersistence persistence, ILogger<AppointmentService> logger)
        {
            _persistence = persistence;
            _logger = logger;
        }
        public async Task<AppointmentModel.Response> Add(AppointmentModel.Request appointment) //post
        {
            #region Validaciones
            var doctor = await _persistence.GetById<Doctor>(appointment.doctorId) ?? throw new EntityNotFoundException(nameof(Doctor));

            var slot = await _persistence.GetById<AvailabilitySlot>(appointment.availabilitySlotId) ?? throw new EntityNotFoundException(nameof(AvailabilitySlot));

            if (appointment.patient.dni.Length < 7 || appointment.patient.dni.Length > 10) throw new ValidationException(ErrorCodes.INVALID_DNI_ERROR, nameof(ErrorCodes.INVALID_DNI_ERROR)); //DNI inválido

            var patient = await _persistence.First<Patient>(p => p.Dni == appointment.patient.dni && p.Deleted == false) ?? throw new EntityNotFoundException(nameof(Patient));

            if (slot.Status != AvailabilitySlotStatus.Available)
            {
                _logger.LogError($"Error en la reserva. Slot {appointment.availabilitySlotId} ya ocupado");
                throw new ConflictException(ErrorCodes.SLOT_NOT_AVAILABLE_CONFLICT, nameof(ErrorCodes.SLOT_NOT_AVAILABLE_CONFLICT)).WithDetail("Slot_Status", "Slot_Not_Available");
            }

            if (slot.SlotDate < DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ConflictException(ErrorCodes.PAST_DATE_CONFLICT, nameof(ErrorCodes.PAST_DATE_CONFLICT)).WithDetail("Slot_Date", "Slot_In_Past_Date");
            }
            else if(slot.SlotDate == DateOnly.FromDateTime(DateTime.Now) && slot.StartTime < TimeOnly.FromDateTime(DateTime.Now))
            {
                throw new ConflictException(ErrorCodes.PAST_TIME_CONFLICT, nameof(ErrorCodes.PAST_TIME_CONFLICT)).WithDetail("Slot_Start_Time", "Slot_In_Past_Time");
            }

            if (appointment.reason.Length < 5 || appointment.reason.Length > 300) throw new ValidationException(ErrorCodes.INVALID_REASON_ERROR, nameof(ErrorCodes.INVALID_REASON_ERROR)); //Motivo inválido
            #endregion

            slot.Status = AvailabilitySlotStatus.Booked; //se registra el appoint como reservado

            await _persistence.Update(slot);
            await _persistence.Add(new Appointment(appointment.reason, slot.Id, patient.Id));

            _logger.LogInformation($"Turno reservado para el patient: {patient.Dni}, con el médico {doctor.Name} en el día {slot.SlotDate} y hora {slot.StartTime} - {slot.EndTime}");

            return new AppointmentModel.Response(doctor.Id, doctor.Name, patient.Dni, patient.Name, slot.StartTime, slot.EndTime);
        }

        public async Task<IEnumerable<AppointmentModel.Response>> GetActiveAppointmentsByPatientDni(string dni) { 

            var patient = await _persistence.First<Patient>(p => p.Dni == dni && p.Deleted == false) ??
                throw new EntityNotFoundException(nameof(Patient));

            var activeAppointments = await _persistence.GetFiltered<Appointment>(a => a.PatientId == patient.Id && a.Status == AppointmentStatus.Booked);

            var appointmentsList = new List<AppointmentModel.Response>();

            if (activeAppointments == null || activeAppointments.Count() == 0) return new List<AppointmentModel.Response>();

            var slot = await _persistence.GetById<AvailabilitySlot>(activeAppointments.First().SlotId);

            foreach (var appoint in activeAppointments)
            {
                appointmentsList.Add(new AppointmentModel.Response(appoint.Slot.Availability.DoctorId, appoint.Slot.Availability.Doctor.Name, dni,patient.Name,appoint.Slot.StartTime,appoint.Slot.EndTime));
            }

            return appointmentsList;
        }

        public async Task CancelAppointment(Guid id) 
        {
            #region Validaciones
            var appointment = await _persistence.GetById<Appointment>(id)
                ?? throw new EntityNotFoundException(nameof(Appointment));

            if (appointment.Status == AppointmentStatus.Cancelled) throw new ConflictException(ErrorCodes.INVALID_APPOINTMENT_STATUS, nameof(ErrorCodes.INVALID_APPOINTMENT_STATUS)).WithDetail("appointmentStatus", "status_already_cancelled");

            if (appointment.Status != AppointmentStatus.Booked) throw new ValidationException(ErrorCodes.INVALID_APPOINTMENT_ERROR,nameof(ErrorCodes.INVALID_APPOINTMENT_ERROR));
            #endregion

            appointment.Status = AppointmentStatus.Cancelled;

            await _persistence.Update(appointment);

            var slot = await _persistence.GetById<AvailabilitySlot>(appointment.SlotId);

            if (slot != null) 
            {
                slot.Status = AvailabilitySlotStatus.Available;

                var now = DateTime.Now;
                slot.UpdatedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

                await _persistence.Update(slot);
                _logger.LogInformation($"Turno cancelado para el patient: {appointment.Patient.Dni}, con el médico {appointment.Slot.Availability.Doctor.Name} en el día {slot.SlotDate} y hora {slot.StartTime} - {slot.EndTime}");
            }

        }

        public async Task<List<AppointmentModel.Response>> GetByDate(DateOnly date)
        {
            var slots = await _persistence.GetFiltered<AvailabilitySlot>(s => s.SlotDate == date && s.Status == AvailabilitySlotStatus.Booked)
                ?? throw new EntityNotFoundException(nameof(AvailabilitySlot));

            var appointsByDay = new List<AppointmentModel.Response>();

            foreach (var slot in slots)
            {
                var availability = await _persistence.GetById<Availability>(slot.AvailabilityId);

                var doctor = await _persistence.GetById<Doctor>(availability.DoctorId);

                var appointment = await _persistence.First<Appointment>(a => a.SlotId == slot.Id && a.Status == AppointmentStatus.Booked);

                var patient = await _persistence.GetById<Patient>(appointment.PatientId);

                appointsByDay.Add(new AppointmentModel.Response(doctor.Id, doctor.Name, patient.Dni, patient.Name, slot.StartTime, slot.EndTime));
            }

            return appointsByDay;
        }

        public async Task<Pagination<AppointmentModel.ResponseGetBySearch>> GetBySearch(int pageSize, int pageIndex, Guid? specialtyId = null, Guid? doctorId = null, string? dni = null, DateOnly? date = null)
        {
            var appointments = await _persistence.Paginate<Appointment, DateOnly>(pageSize, pageIndex,
                        a =>
                            (!doctorId.HasValue || a.Slot.Availability.DoctorId == doctorId) &&
                            (!specialtyId.HasValue || a.Slot.Availability.Doctor.Specialty.Id == specialtyId) &&
                            (string.IsNullOrWhiteSpace(dni) || a.Patient.Dni == dni) &&
                            (!date.HasValue || a.Slot.SlotDate == date),

                        a => a.Slot.SlotDate,

                        nameof(Appointment.Patient),
                        nameof(Appointment.Slot),
                        $"{nameof(Appointment.Slot)}.{nameof(AvailabilitySlot.Availability)}",
                        $"{nameof(Appointment.Slot)}.{nameof(AvailabilitySlot.Availability)}.{nameof(Availability.Doctor)}",
                        $"{nameof(Appointment.Slot)}.{nameof(AvailabilitySlot.Availability)}.{nameof(Availability.Doctor)}.{nameof(Doctor.Specialty)}"
                        );

            return appointments.Map(c => new AppointmentModel.ResponseGetBySearch(c.Id, c.Status, 
                                    new AppointmentModel.PatientResponse(c.Patient.Dni,c.Patient.Name),
                                    new AppointmentModel.DoctorResponse(c.Slot.Availability.Doctor.Id, c.Slot.Availability.Doctor.Name, 
                                        new AppointmentModel.SpecialtyResponse(c.Slot.Availability.Doctor.Specialty.Id, c.Slot.Availability.Doctor.Specialty.Name))));

        }  

    }
}