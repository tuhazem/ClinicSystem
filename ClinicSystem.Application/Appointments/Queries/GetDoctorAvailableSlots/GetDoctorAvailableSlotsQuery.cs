using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Appointments;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Appointments.Queries.GetDoctorAvailableSlots;

public record AvailableSlotDto(
    DateTime StartTimeUtc,
    DateTime EndTimeUtc,
    bool IsAvailable
);

public record GetDoctorAvailableSlotsQuery(
    Guid DoctorId,
    DateTime DateUtc
) : IRequest<IReadOnlyList<AvailableSlotDto>>, ICachableQuery<IReadOnlyList<AvailableSlotDto>>
{
    public string CacheKey => $"doctor-{DoctorId}-slots-{DateUtc:yyyyMMdd}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(2);
    public IReadOnlyCollection<string>? Tags => ["appointments", $"doctor-{DoctorId}-appointments"];
}

public class GetDoctorAvailableSlotsQueryHandler(
    IDoctorRepository doctorRepository,
    IAppointmentRepository appointmentRepository)
    : IRequestHandler<GetDoctorAvailableSlotsQuery, IReadOnlyList<AvailableSlotDto>>
{
    public async Task<IReadOnlyList<AvailableSlotDto>> Handle(
        GetDoctorAvailableSlotsQuery request,
        CancellationToken cancellationToken)
    {
        var doctor = await doctorRepository.GetByIdAsync(request.DoctorId, cancellationToken);
        if (doctor == null)
        {
            throw new KeyNotFoundException($"Doctor with ID '{request.DoctorId}' was not found.");
        }

        var targetDate = request.DateUtc.Date;
        var dayOfWeek = targetDate.DayOfWeek;

        // Check if doctor has specific schedule for this day of week
        var schedule = doctor.WorkingSchedules.FirstOrDefault(s => s.DayOfWeek == dayOfWeek && s.IsActive);

        // Default clinic hours if no specific schedule: 09:00 - 17:00, 30 min slots
        var startTime = schedule?.StartTime ?? new TimeSpan(9, 0, 0);
        var endTime = schedule?.EndTime ?? new TimeSpan(17, 0, 0);
        var slotDurationMinutes = schedule?.SlotDurationMinutes ?? 30;

        // Fetch existing appointments on that date
        var existingAppointments = await appointmentRepository.GetByDoctorIdAsync(
            request.DoctorId,
            targetDate,
            cancellationToken);

        var activeAppointments = existingAppointments
            .Where(a => a.Status != AppointmentStatus.Cancelled)
            .ToList();

        var slots = new List<AvailableSlotDto>();
        var currentSlotStart = targetDate.Add(startTime);
        var dayEnd = targetDate.Add(endTime);

        while (currentSlotStart.AddMinutes(slotDurationMinutes) <= dayEnd)
        {
            var currentSlotEnd = currentSlotStart.AddMinutes(slotDurationMinutes);

            var isOverlapping = activeAppointments.Any(a =>
                a.ScheduledStartTimeUtc < currentSlotEnd && a.ScheduledEndTimeUtc > currentSlotStart);

            slots.Add(new AvailableSlotDto(
                currentSlotStart,
                currentSlotEnd,
                !isOverlapping
            ));

            currentSlotStart = currentSlotEnd;
        }

        return slots;
    }
}
