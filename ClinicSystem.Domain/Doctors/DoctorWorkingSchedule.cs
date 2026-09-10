using ClinicSystem.Domain.Common;
using System;

namespace ClinicSystem.Domain.Doctors;

public class DoctorWorkingSchedule : BaseEntity
{
    public Guid DoctorId { get; private set; }
    public DayOfWeek DayOfWeek { get; private set; }
    public TimeSpan StartTime { get; private set; }
    public TimeSpan EndTime { get; private set; }
    public int SlotDurationMinutes { get; private set; } = 30;
    public bool IsActive { get; private set; } = true;

    private DoctorWorkingSchedule() { }

    public static DoctorWorkingSchedule Create(
        Guid doctorId,
        DayOfWeek dayOfWeek,
        TimeSpan startTime,
        TimeSpan endTime,
        int slotDurationMinutes = 30)
    {
        if (doctorId == Guid.Empty)
            throw new ArgumentException("Doctor ID is required.", nameof(doctorId));

        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time.", nameof(endTime));

        if (slotDurationMinutes <= 0 || slotDurationMinutes > 240)
            throw new ArgumentException("Slot duration must be between 1 and 240 minutes.", nameof(slotDurationMinutes));

        return new DoctorWorkingSchedule
        {
            Id = Guid.NewGuid(),
            DoctorId = doctorId,
            DayOfWeek = dayOfWeek,
            StartTime = startTime,
            EndTime = endTime,
            SlotDurationMinutes = slotDurationMinutes,
            IsActive = true
        };
    }

    public void UpdateSchedule(TimeSpan startTime, TimeSpan endTime, int slotDurationMinutes, bool isActive = true)
    {
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time.", nameof(endTime));

        if (slotDurationMinutes <= 0 || slotDurationMinutes > 240)
            throw new ArgumentException("Slot duration must be between 1 and 240 minutes.", nameof(slotDurationMinutes));

        StartTime = startTime;
        EndTime = endTime;
        SlotDurationMinutes = slotDurationMinutes;
        IsActive = isActive;
        UpdateModifiedTime();
    }
}
