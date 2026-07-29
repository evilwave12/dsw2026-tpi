using Azure;
using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
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

            slot.Status = AvailabilitySlotStatus.Booked; //se registra el turno como reservado

            await _persistence.Update(slot);
            
            return await _persistence.Add(new Appointment(cita.reason, slot.Id, paciente.Id));
        }

        public async Task<IEnumerable<AppointmentModel.Response>> GetActiveAppointmentsByPatientDni(string dni) { 

            var paciente = await _persistence.First<Patient>(p => p.Dni == dni && p.Deleted == false) ??
                throw new EntityNotFoundException(nameof(Patient)); 

            var activeAppointments = await _persistence.GetFiltered<Appointment>(
                a => a.Patient_Id == paciente.Id &&
                a.Status == AppointmentStatus.Booked
                );

            var lista = new List<AppointmentModel.Response>();

            foreach (var turno in activeAppointments)
            {
                lista.Add(new AppointmentModel.Response(turno.Slot.Availability.Doctor_Id,dni,paciente.Name,turno.Slot.Start_time,turno.Slot.End_time));
            }

            return lista ?? new List<AppointmentModel.Response>();
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

                var appointment = await _persistence.First<Appointment>(a => a.Slot_Id == slot.Id && a.Status == AppointmentStatus.Booked);

                var paciente = await _persistence.GetById<Patient>(appointment.Patient_Id);

                turnosdia.Add(new AppointmentModel.Response(doctor.Id, paciente.Dni, paciente.Name, slot.Start_time, slot.End_time));
            }

            return turnosdia;
        }

        public async Task<Pagination<AppointmentModel.ResponseGetBySearch>> GetBySearch(int pageSize, int pageIndex, Guid? specialityId = null, Guid? doctorId = null,
                                                                                       string? dni = null,
                                                                                       DateOnly? date = null)
        {

            var citas = await _persistence.Paginate<Appointment, DateOnly>(pageSize, pageIndex,
                        a =>
                            (!doctorId.HasValue || a.Slot.Availability.Doctor_Id == doctorId) &&
                            (!specialityId.HasValue || a.Slot.Availability.Doctor.Speciality.Id == specialityId) &&
                            (string.IsNullOrWhiteSpace(dni) || a.Patient.Dni == dni) &&
                            (!date.HasValue || a.Slot.Slot_date == date),

                        a => a.Slot.Slot_date,

                        nameof(Appointment.Patient),
                        nameof(Appointment.Slot),
                        $"{nameof(Appointment.Slot)}.{nameof(AvailabilitySlot.Availability)}",
                        $"{nameof(Appointment.Slot)}.{nameof(AvailabilitySlot.Availability)}.{nameof(Availability.Doctor)}",
                        $"{nameof(Appointment.Slot)}.{nameof(AvailabilitySlot.Availability)}.{nameof(Availability.Doctor)}.{nameof(Doctor.Speciality)}"
                        );


            return citas.Map(c => new AppointmentModel.ResponseGetBySearch(c.Slot.Availability.Doctor.Speciality.Name, c.Slot.Availability.Doctor.Name, c.Slot.Slot_date, c.Slot.Start_time, c.Slot.End_time));

            /*var query = from appointment in _persistence.Query<Appointment>()

                join patient in _persistence.Query<Patient>() on appointment.Patient_Id equals patient.Id

                join slot in _persistence.Query<AvailabilitySlot>() on appointment.Slot_Id equals slot.Id

                join availability in _persistence.Query<Availability>() on slot.AvailabilityId equals availability.Id

                join doctor in _persistence.Query<Doctor>() on availability.Doctor_Id equals doctor.Id

                join speciality in _persistence.Query<Speciality>() on doctor.SpecialityId equals speciality.Id

                select new
                {
                    Appointment = appointment,
                    Patient = patient,
                    Slot = slot,
                    Doctor = doctor,
                    Speciality = speciality
                };

            if (doctorId.HasValue)
                query = query.Where(x => x.Doctor.Id == doctorId);

            if (specialityId.HasValue)
                query = query.Where(x => x.Speciality.Id == specialityId);

            if (!string.IsNullOrWhiteSpace(dni))
                query = query.Where(x => x.Patient.Dni == dni);

            if (date.HasValue)
                query = query.Where(x => x.Slot.Slot_date == date);


            var queryOrdenada = query.OrderBy(x => x.Slot.Slot_date).ThenBy(x => x.Slot.Start_time);

            var citas = await _persistence.Paginate(pageSize, pageIndex, queryOrdenada);

            return citas.Map(x => new AppointmentModel.ResponseGetBySearch(x.Speciality.Name, x.Doctor.Name, x.Slot.Slot_date, x.Slot.Start_time, x.Slot.End_time));*/
        }

    }
}
