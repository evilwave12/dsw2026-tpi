using Dsw2026Tpi.Application.Dtos;
using Dsw2026Tpi.Application.Interfaces;
using Dsw2026Tpi.CrossCutting.Exceptions;
using Dsw2026Tpi.CrossCutting.Resources;
using Dsw2026Tpi.Domain.Entities;
using Dsw2026Tpi.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using static Dsw2026Tpi.Application.Dtos.AvailabilityModel;

namespace Dsw2026Tpi.Application.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IPersistence _persistence;
        private readonly ILogger<AvailabilityService> _logger;

        public AvailabilityService(IPersistence persistence, ILogger<AvailabilityService> logger)
        {
            _persistence = persistence;
            _logger = logger;
        }

        public async Task<IEnumerable<AvailabilityModel.Response>> CreateAvailabilitiesAsync(AvailabilityModel.Request request)
        {

            //cargar los slots desde las availabilities existentes por el json
            var availabilitiesTemporal = await _persistence.GetAll<Availability>();
            if(availabilitiesTemporal is not null)
            {
                foreach (var dispo in availabilitiesTemporal)
                {
                    await GenerateAndSaveSlotsAsync(dispo, DateTime.Now);
                }
            }
            ////////////////////////////////////////////////

            var doctor = await _persistence.GetById<Doctor>(request.DoctorId);

            if (doctor == null || doctor.Deleted) throw new EntityNotFoundException(nameof(Doctor));
            
            var currentDate = DateTime.Now;
            var currentMonth = (byte)currentDate.Month;
            var currentYear = (short)currentDate.Year;

            var availabilities = new List<AvailabilityModel.Response>(); //ir guardando las availabilities creadas para devolverlas al final

            foreach (var dayRequest in request.Days)
            {
                if (dayRequest.startTime >= dayRequest.endTime) throw new ConflictException(ErrorCodes.INVALID_TIME_CONFLICT, nameof(ErrorCodes.INVALID_TIME_CONFLICT)).WithDetail("startTime", "startTime_after_endTime");
                
                var dayOfWeekNumber = MapStringToDayOfWeekNumber(dayRequest.dayOfTheWeek);

                var availability = new Availability(request.DoctorId, currentMonth, currentYear, dayOfWeekNumber, dayRequest.startTime, dayRequest.endTime);

                var day= ((DiaSemana)dayOfWeekNumber).ToString(); //formateao pa la salida

                availabilities.Add(new AvailabilityModel.Response(day, $"{availability.StartTime:HH:mm}", $"{availability.EndTime:HH:mm}"));
            }
            await ValidationAvailabilitiesAsync(request);

            _logger.LogInformation($"Disponibilidades creadas exitosamente para el médico {doctor.Name}, id: {doctor.Id}");

            return availabilities;
        }

        public async Task ValidationAvailabilitiesAsync(AvailabilityModel.Request request)
        {
            var currentDate = DateTime.Now;
            var currentMonth = (byte)currentDate.Month;
            var currentYear = (short)currentDate.Year;
            var currentDay = (byte)currentDate.Day;

            foreach(var dayRequest in request.Days)
            {
                var dayNumber = MapStringToDayOfWeekNumber(dayRequest.dayOfTheWeek);

                var currentAvailability = await _persistence.GetFiltered<Availability>(
                a => a.DoctorId == request.DoctorId &&
                     a.Month == currentMonth &&
                     a.Year == currentYear &&
                     a.DayOfTheWeek == dayNumber
                );

                if (currentAvailability.Count() != 0 && !(currentAvailability is null)) //si se está intentando crear una availability en un dia donde un doctor ya tiene horario
                {
                    var existingList = currentAvailability?.ToList() ?? new List<Availability>();
                    var sameDayAvailabilities = existingList.Where(a => a.DayOfTheWeek == dayNumber);

                        foreach (var existing in sameDayAvailabilities)
                        {
                            bool taSolapado = dayRequest.startTime < existing.EndTime &&
                                              dayRequest.endTime > existing.StartTime;

                            if (taSolapado) throw new ConflictException(ErrorCodes.OVERLAP_CONFLICT, nameof(ErrorCodes.OVERLAP_CONFLICT)); //no se como hacerle los details a este
                        }
                }
            }

            //si llega aqui es que no hay ningun conflicto de solapamiento
            foreach (var dayRequest in request.Days)
            {
                var dayNumber = MapStringToDayOfWeekNumber(dayRequest.dayOfTheWeek);
                var newAvailability = new Availability(request.DoctorId, currentMonth, currentYear, dayNumber, dayRequest.startTime, dayRequest.endTime);
                /*uwu*/

                await _persistence.Add(newAvailability);
                await GenerateAndSaveSlotsAsync(newAvailability, currentDate);
            }    
        }

        private async Task GenerateAndSaveSlotsAsync(Availability rule, DateTime currentDate)
        {
            var holidays = await GetHolidaysAsync();
            var holidaysDates = holidays.Select(f => f.Date);

            int daysInMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month);

            for (int day = currentDate.Day; day <= daysInMonth; day++)
            {
                var dateToProcess = new DateOnly(currentDate.Year, currentDate.Month, day);

                if (holidaysDates.Contains(dateToProcess)) {
                    continue;
                }

                if ((byte)dateToProcess.DayOfWeek == rule.DayOfTheWeek)
                {
                    var slotStart = rule.StartTime;

                    while (slotStart.AddMinutes(30) <= rule.EndTime && !slotStart.Equals(rule.EndTime)) //la ultima validacion es para evitar un bucle infinito en casos como 23:30 - 00:00
                    {
                        var slotEnd = slotStart.AddMinutes(30);

                        var newSlot = new AvailabilitySlot(rule.Id, dateToProcess, slotStart, slotEnd);

                        await _persistence.Add(newSlot);   

                        slotStart = slotEnd;
                    }
                }
            }
        }

        private async Task<List<HolidayModel>> GetHolidaysAsync() 
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "Sources\\holidays.json");

            if (!File.Exists(filePath)) 
            { 
                return new List<HolidayModel>();
            }

            var jsonContent = await File.ReadAllTextAsync(filePath);

            var options = new JsonSerializerOptions 
                { 
                PropertyNameCaseInsensitive = true
                };

            return JsonSerializer.Deserialize<List<HolidayModel>>(jsonContent,options) ?? new List<HolidayModel>();
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
                _ => throw new ValidationException(ErrorCodes.INVALID_DAY_ERROR, nameof(ErrorCodes.INVALID_DAY_ERROR))
            };
        }

        public async Task<IEnumerable<AvailabilityModel.Response>> Update(AvailabilityModel.Request request) //mismo codigo que post pero borramos todos los registros y escribimos nuevos ya con las modificaciones
        {
            var doctor = await _persistence.GetById<Doctor>(request.DoctorId) ?? throw new EntityNotFoundException(nameof(Doctor)); ;

            if (!doctor.IsActive) throw new ValidationException(ErrorCodes.DOCTOR_INACTIVE, nameof(ErrorCodes.DOCTOR_INACTIVE));
            
            var availabilities = await _persistence.GetFiltered<Availability>(a => a.DoctorId == request.DoctorId) ?? throw new EntityNotFoundException(nameof(Availability));

            bool flag = false;

            foreach (var dispo in availabilities)
            {
                var slots = await _persistence.GetFiltered<AvailabilitySlot>(s => s.AvailabilityId == dispo.Id);

                foreach (var slot in slots)
                {
                    if (slot.Status != AvailabilitySlotStatus.Booked)
                    {
                        await _persistence.Delete(slot);
                    }
                    else { flag = true; }
                }

                if(!flag) await _persistence.Delete(dispo); //borra la availability solo si no tiene algun slot reservado
            }

            var currentDate = DateTime.Now;
            var currentMonth = (byte)currentDate.Month;
            var currentYear = (short)currentDate.Year;

            var availabilities2 = new List<AvailabilityModel.Response>(); //ir guardando las availabilities creadas para devolverlas al final

            foreach (var dayRequest in request.Days)
            {

                if (dayRequest.startTime >= dayRequest.endTime)
                {
                    throw new ConflictException(ErrorCodes.INVALID_TIME_CONFLICT, nameof(ErrorCodes.INVALID_TIME_CONFLICT)).WithDetail("startTime", "startTime_after_endTime");
                }

                var dayOfWeekNumber = MapStringToDayOfWeekNumber(dayRequest.dayOfTheWeek);

                var availability = new Availability(request.DoctorId, currentMonth, currentYear, dayOfWeekNumber, dayRequest.startTime, dayRequest.endTime);

                var day= ((DiaSemana)dayOfWeekNumber).ToString(); //formateao pa la salida
                availabilities2.Add(new AvailabilityModel.Response(day, $"{availability.StartTime:HH:mm}", $"{availability.EndTime:HH:mm}"));
            }
            await ValidationAvailabilitiesAsync(request);
            return availabilities2;
        }

        public async Task<IEnumerable<AvailabilityModel.ResponseAvailabilitySlots>> GetAvailabilitySlots(Guid idAvailability)
        {
            var availability = await _persistence.GetById<Availability>(idAvailability);

            if (availability is null) throw new EntityNotFoundException(nameof(Availability));

            var slots = await _persistence.GetFiltered<AvailabilitySlot>(s => s.AvailabilityId == idAvailability && s.Status == AvailabilitySlotStatus.Available);

            return slots.Select(s => new AvailabilityModel.ResponseAvailabilitySlots(s.Id, s.SlotDate, s.StartTime, s.EndTime));
        }
    }

}
