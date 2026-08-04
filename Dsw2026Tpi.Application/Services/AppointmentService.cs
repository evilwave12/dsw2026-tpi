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
        public async Task<AppointmentModel.Response> Add(AppointmentModel.Request cita) //post
        {
            var doctor = await _persistence.GetById<Doctor>(cita.doctorId) ?? throw new EntityNotFoundException(nameof(Doctor));

            var slot = await _persistence.GetById<AvailabilitySlot>(cita.availabilitySlotId) ?? throw new EntityNotFoundException(nameof(AvailabilitySlot));

            if (cita.patient.dni.Length < 7 || cita.patient.dni.Length > 10) throw new ValidationException(ErrorCodes.INVALID_DNI_ERROR, nameof(ErrorCodes.INVALID_DNI_ERROR)); //DNI inválido

            var paciente = await _persistence.First<Patient>(p => p.Dni == cita.patient.dni && p.Deleted == false) ?? throw new EntityNotFoundException(nameof(Patient));

            if (slot.Status != AvailabilitySlotStatus.Available)
            {
                _logger.LogError($"Error en la reserva. Slot {cita.availabilitySlotId} ya ocupado");

                throw new ConflictException(ErrorCodes.SLOT_NOT_AVAILABLE_CONFLICT, nameof(ErrorCodes.SLOT_NOT_AVAILABLE_CONFLICT)).WithDetail("Slot_Status", "Slot_Not_Available");

            }
          
            if (slot.Slot_date < DateOnly.FromDateTime(DateTime.Now)) throw new ConflictException(ErrorCodes.PAST_DATE_CONFLICT, nameof(ErrorCodes.PAST_DATE_CONFLICT)).WithDetail("Slot_Date", "Slot_In_Past_Date");
            
            if (cita.reason.Length < 5 || cita.reason.Length > 300) throw new ValidationException(ErrorCodes.INVALID_REASON_ERROR, nameof(ErrorCodes.INVALID_REASON_ERROR)); //Motivo inválido
            
            slot.Status = AvailabilitySlotStatus.Booked; //se registra el turno como reservado

            await _persistence.Update(slot);
            await _persistence.Add(new Appointment(cita.reason, slot.Id, paciente.Id));

            _logger.LogInformation($"Turno reservado para el paciente: {paciente.Dni}, con el médico {doctor.Name} en el día {slot.Slot_date} y hora {slot.Start_time} - {slot.End_time}");

            return new AppointmentModel.Response(doctor.Id, doctor.Name, paciente.Dni, paciente.Name, slot.Start_time, slot.End_time);
        }

        public async Task<IEnumerable<AppointmentModel.Response>> GetActiveAppointmentsByPatientDni(string dni) { 

            var paciente = await _persistence.First<Patient>(p => p.Dni == dni && p.Deleted == false) ??
                throw new EntityNotFoundException(nameof(Patient));

            var activeAppointments = await _persistence.GetFiltered<Appointment>(a => a.Patient_Id == paciente.Id && a.Status == AppointmentStatus.Booked);

            var listaTurnos = new List<AppointmentModel.Response>();

            if (activeAppointments == null || activeAppointments.Count() == 0) return new List<AppointmentModel.Response>();

            var slot = await _persistence.GetById<AvailabilitySlot>(activeAppointments.First().Slot_Id);

            foreach (var turno in activeAppointments)
            {
                listaTurnos.Add(new AppointmentModel.Response(turno.Slot.Availability.Doctor_Id, turno.Slot.Availability.Doctor.Name, dni,paciente.Name,turno.Slot.Start_time,turno.Slot.End_time));
            }

            return listaTurnos;
        }

        public async Task CancelAppointment(Guid id) {

            var appointment = await _persistence.GetById<Appointment>(id)
                ?? throw new EntityNotFoundException(nameof(Appointment));

            if (appointment.Status == AppointmentStatus.Cancelled) throw new ConflictException(ErrorCodes.INVALID_APPOINTMENT_STATUS, nameof(ErrorCodes.INVALID_APPOINTMENT_STATUS)).WithDetail("appointmentStatus", "status_already_cancelled");

            if (appointment.Status != AppointmentStatus.Booked) throw new ValidationException(ErrorCodes.INVALID_APPOINTMENT_ERROR,nameof(ErrorCodes.INVALID_APPOINTMENT_ERROR));
            
            appointment.Status = AppointmentStatus.Cancelled;

            await _persistence.Update(appointment);

            var slot = await _persistence.GetById<AvailabilitySlot>(appointment.Slot_Id);

            if (slot != null) 
            {
                slot.Status = AvailabilitySlotStatus.Available;

                var now = DateTime.Now;
                slot.UpdatedAt = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);

                await _persistence.Update(slot);
                _logger.LogInformation($"Turno cancelado para el paciente: {appointment.Patient.Dni}, con el médico {appointment.Slot.Availability.Doctor.Name} en el día {slot.Slot_date} y hora {slot.Start_time} - {slot.End_time}");
            }

        }

        public async Task<List<AppointmentModel.Response>> GetByDate(DateOnly date)
        {
            var slots = await _persistence.GetFiltered<AvailabilitySlot>(s => s.Slot_date == date && s.Status == AvailabilitySlotStatus.Booked)
                ?? throw new EntityNotFoundException(nameof(AvailabilitySlot));

            var turnosdia = new List<AppointmentModel.Response>();

            foreach (var slot in slots)
            {
                var disponibilidad = await _persistence.GetById<Availability>(slot.AvailabilityId);

                var doctor = await _persistence.GetById<Doctor>(disponibilidad.Doctor_Id);

                var appointment = await _persistence.First<Appointment>(a => a.Slot_Id == slot.Id && a.Status == AppointmentStatus.Booked);

                var paciente = await _persistence.GetById<Patient>(appointment.Patient_Id);

                turnosdia.Add(new AppointmentModel.Response(doctor.Id, doctor.Name, paciente.Dni, paciente.Name, slot.Start_time, slot.End_time));
            }

            return turnosdia;
        }

        public async Task<Pagination<AppointmentModel.ResponseGetBySearch>> GetBySearch(int pageSize, int pageIndex, Guid? specialtyId = null, Guid? doctorId = null, string? dni = null, DateOnly? date = null)
        {
            var citas = await _persistence.Paginate<Appointment, DateOnly>(pageSize, pageIndex,
                        a =>
                            (!doctorId.HasValue || a.Slot.Availability.Doctor_Id == doctorId) &&
                            (!specialtyId.HasValue || a.Slot.Availability.Doctor.Specialty.Id == specialtyId) &&
                            (string.IsNullOrWhiteSpace(dni) || a.Patient.Dni == dni) &&
                            (!date.HasValue || a.Slot.Slot_date == date),

                        a => a.Slot.Slot_date,

                        nameof(Appointment.Patient),
                        nameof(Appointment.Slot),
                        $"{nameof(Appointment.Slot)}.{nameof(AvailabilitySlot.Availability)}",
                        $"{nameof(Appointment.Slot)}.{nameof(AvailabilitySlot.Availability)}.{nameof(Availability.Doctor)}",
                        $"{nameof(Appointment.Slot)}.{nameof(AvailabilitySlot.Availability)}.{nameof(Availability.Doctor)}.{nameof(Doctor.Specialty)}"
                        );

            return citas.Map(c => new AppointmentModel.ResponseGetBySearch(c.Id, c.Status, 
                                    new AppointmentModel.PatientResponse(c.Patient.Dni,c.Patient.Name),
                                    new AppointmentModel.DoctorResponse(c.Slot.Availability.Doctor.Id, c.Slot.Availability.Doctor.Name, 
                                        new AppointmentModel.SpecialtyResponse(c.Slot.Availability.Doctor.Specialty.Id, c.Slot.Availability.Doctor.Specialty.Name))));

        }  

    }
}