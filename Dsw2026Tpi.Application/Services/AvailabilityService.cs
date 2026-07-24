using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Dsw2026Tpi.Application.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IPersistence _persistence;

        public AvailabilityService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task<IEnumerable<AvailabilityModel.Response>> CreateAvailabilitiesAsync(AvailabilityModel.Request request)
        {
            var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
            if (doctor == null || !doctor.IsActive)
            {
                throw new EntityNotFoundException(nameof(Doctor));
            }

            var currentDate = DateTime.Now;
            var currentMonth = (byte)currentDate.Month;
            var currentYear = (short)currentDate.Year;

            var availabilities = new List<AvailabilityModel.Response>(); //ir guardando las disponibilidades creadas para devolverlas al final

            foreach (var dayRequest in request.Days)
            {

                if (dayRequest.StartTime >= dayRequest.EndTime)
                {
                    throw new ValidationException();
                }

                var dayOfWeekNumber = MapStringToDayOfWeekNumber(dayRequest.Day);

                var availability = new Availability(request.DoctorId, currentMonth, currentYear, dayOfWeekNumber, dayRequest.StartTime, dayRequest.EndTime);

                await ValidationAvailabilitiesAsync(request,doctor);

                var DIA = ((DiaSemana)dayOfWeekNumber).ToString(); //formateao pa la salida
                availabilities.Add(new AvailabilityModel.Response(DIA, $"{availability.Start_time:HH:mm}", $"{availability.End_time:HH:mm}"));
            }
            return availabilities;
        }

        public async Task ValidationAvailabilitiesAsync(AvailabilityModel.Request request, Doctor doctor)
        {
            var currentDate = DateTime.Now;
            var currentMonth = (byte)currentDate.Month;
            var currentYear = (byte)currentDate.Year;
            var currentDay = (byte)currentDate.Day;

            var currentAvailabilities = await _persistence.GetFiltered<Availability>(
                a => a.Doctor_Id == request.DoctorId &&
                     a.Month == currentMonth &&
                     a.Year == currentYear
            );

            if (currentAvailabilities != null) //si se está intentando crear una disponibilidad en un dia donde un doctor ya tiene horario
            {
                foreach (var newDayRequest in request.Days)
                {

                    var numerin = MapStringToDayOfWeekNumber(newDayRequest.Day);
                    var existingList = currentAvailabilities?.ToList() ?? new List<Availability>();
                    var mismoDiaExistenteDisponilidades = existingList
                        .Where(a => a.Day_of_the_week==numerin);

                    foreach (var existente in mismoDiaExistenteDisponilidades)
                    {
                        bool taSolapado = newDayRequest.StartTime < existente.End_time &&
                                          newDayRequest.EndTime > existente.Start_time;

                        if (taSolapado)
                        {
                            throw new ValidationException(); 
                        }
                    }
                }
                
                foreach (var newDayRequest in request.Days) //si llega aqui es que no hay ningun conflicto de solapamiento
                {
                    var numerin = MapStringToDayOfWeekNumber(newDayRequest.Day);
                    var newAvailability = new Availability
                    (request.DoctorId,currentMonth,currentYear,numerin,newDayRequest.StartTime,newDayRequest.EndTime);
                    /*uwu*/
                    await _persistence.Add(newAvailability);

                    await GenerateAndSaveSlotsAsync(newAvailability, currentDate);
                }
            }
        }

        private async Task GenerateAndSaveSlotsAsync(Availability rule, DateTime currentDate)
        {
            int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);

            for (int day = currentDate.Day; day <= daysInMonth; day++)
            {
                var dateToProcess = new DateOnly(currentDate.Year, currentDate.Month, day);

                if ((int)dateToProcess.DayOfWeek == rule.Day_of_the_week)
                {
                    var slotStart = rule.Start_time;

                    while (slotStart.AddMinutes(30) <= rule.End_time)
                    {
                        var slotEnd = slotStart.AddMinutes(30);

                        var newSlot = new AvailabilitySlot(rule.Id, dateToProcess, slotStart, slotEnd);

                        await _persistence.Add(newSlot);
                        slotStart = slotEnd;
                    }
                }
            }
        }
        private byte MapStringToDayOfWeekNumber(string day)
        {
            return day.ToLower() switch
            {
                "domingo" => (byte)DayOfWeek.Sunday,
                "lunes" => (byte)DayOfWeek.Monday,
                "martes" => (byte)DayOfWeek.Tuesday,
                "miercoles" or "miércoles" => (byte)DayOfWeek.Wednesday,
                "jueves" => (byte)DayOfWeek.Thursday,
                "viernes" => (byte)DayOfWeek.Friday,
                "sabado" or "sábado" => (byte)DayOfWeek.Saturday,
                _ => throw new ValidationException()
            };
        }
    }
}
