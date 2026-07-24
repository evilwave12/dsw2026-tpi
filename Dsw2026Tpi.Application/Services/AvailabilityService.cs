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
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IPersistence _persistence;

        public AvailabilityService(IPersistence persistence)
        {
            _persistence = persistence;
        }

        public async Task CreateAvailabilitiesAsync(AvailabilityModel.Request request)
        {
            var doctor = await _persistence.GetById<Doctor>(request.DoctorId);
            if (doctor == null || !doctor.IsActive)
            {
                throw new EntityNotFoundException(nameof(Doctor));
            }

            var currentDate = DateTime.Now;
            var currentMonth = (byte)currentDate.Month;
            var currentYear = (short)currentDate.Year;

            foreach (var dayRequest in request.Days)
            {

                if (dayRequest.StartTime >= dayRequest.EndTime)
                {
                    throw new ValidationException();
                }

                var dayOfWeekNumber = MapStringToDayOfWeekNumber(dayRequest.Day);

                var availability = new Availability(currentMonth, currentYear, dayOfWeekNumber, dayRequest.StartTime, dayRequest.EndTime)
                {
                    Doctor_Id = request.DoctorId
                };

                await _persistence.Add(availability);

                await GenerateAndSaveSlotsAsync(availability, currentDate);
            }
        }
        public async Task UpdateAvailabilitiesAsync(AvailabilityModel.Request request)
        {

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

                        var newSlot = new AvailabilitySlot(rule.Id, dateToProcess, slotStart, slotEnd)
                        {
                            //REVISAR SI HAY QUE PONER UN AVAILABILITY_ID=RULE_ID
                        };

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
