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
    public class AppointmentService : IAppointmentService
    {
        private readonly IPersistence _persistence;

        public AppointmentService(IPersistence persistence)
        {
            _persistence = persistence;
        }
        public async Task<Appointment> Add(AppointmentModel.Request cita) //post
        {
            var doctor = await _persistence.GetById<Doctor>(cita.Id_doctor) ?? throw new EntityNotFoundException(nameof(Doctor));
            var disponibilidad = await _persistence.GetById<Availability>(cita.Id_Disponibilidad) ?? throw new EntityNotFoundException(nameof(Availability));
            var slot = await _persistence.First<AvailabilitySlot>(s => s.AvailabilityId == cita.Id_Disponibilidad && s.Start_time == cita.hora.StartTime && s.End_time == cita.hora.EndTime) ?? throw new EntityNotFoundException(nameof(AvailabilitySlot));
            var paciente = await _persistence.First<Patient>(p => p.Dni == cita.dni && p.Deleted == false) ?? throw new EntityNotFoundException(nameof(Patient));

            if(slot.Status != AvailabilitySlotStatus.Available)
            {
                throw new ValidationException(); //turno no disponible
            }

            if(slot.Slot_date < DateOnly.FromDateTime(DateTime.Now))
            {
                throw new ValidationException(); //turno en fecha pasada
            }

            if(cita.dni.Length < 7 || cita.dni.Length > 10)
            {
                throw new ValidationException(); //DNI inválido
            }

            if (cita.reason.Length < 5)
            {
                throw new ValidationException(); //Motivo inválido
            }

            return await _persistence.Add(new Appointment(cita.reason, slot.Id, paciente.Id));
        }

        public async Task<IEnumerable<Appointment>> GetActiveAppointmentsByPatientDni(string dni) { 

            var paciente = await _persistence.First<Patient>(p => p.Dni == dni && p.Deleted == false) ??
                throw new EntityNotFoundException(nameof(Patient)); 

            var activeAppointments = await _persistence.GetFiltered<Appointment>(
                a => a.Patient_Id == paciente.Id &&
                a.Status == AppointmentStatus.Booked
                );

            return activeAppointments ?? new List<Appointment>();
        }

        public async Task CancelAppointment(Guid id) {

            var appointment = await _persistence.GetById<Appointment>(id)
                ?? throw new EntityNotFoundException(nameof(Appointment));

            if (appointment.Status != AppointmentStatus.Booked)
            {
                throw new ValidationException("No se puede cancelar la cita si no esta reservada.",
                    "ESTADO_INVALIDO_TURNO"/*? o INVALID_APPOINTMENT_STATUS*/);
            }

            appointment.Status = AppointmentStatus.Cancelled;

            await _persistence.Update(appointment);

            var slot = await _persistence.GetById<AvailabilitySlot>(appointment.Slot_Id);

            if (slot != null) {
                slot.Status = AvailabilitySlotStatus.Available;
                await _persistence.Update(slot);
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

                var appointment = await _persistence.First<Appointment>(a => a.Slot_Id == slot.Id);

                var paciente = await _persistence.GetById<Patient>(appointment.Patient_Id);

                turnosdia.Add(new AppointmentModel.Response(doctor.Id, paciente.Dni, paciente.Name, slot.Start_time, slot.End_time));
            }

            return turnosdia;
                
            
        }



    }
}
