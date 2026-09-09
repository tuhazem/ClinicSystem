using ClinicSystem.Domain.Common;
using System;

namespace ClinicSystem.Domain.Appointments;

public class Appointment : BaseEntity
{
    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public DateTime ScheduledStartTimeUtc { get; private set; }
    public DateTime ScheduledEndTimeUtc { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public AppointmentType Type { get; private set; }
    public string? ReasonForVisit { get; private set; }
    public string? CancellationReason { get; private set; }
    public int? QueueNumber { get; private set; }

    private Appointment() { }

    public static Appointment Schedule(
        Guid patientId,
        Guid doctorId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        AppointmentType type,
        string? reasonForVisit = null,
        int? queueNumber = null)
    {
        if (patientId == Guid.Empty)
            throw new ArgumentException("Patient ID is required.", nameof(patientId));

        if (doctorId == Guid.Empty)
            throw new ArgumentException("Doctor ID is required.", nameof(doctorId));

        if (endTimeUtc <= startTimeUtc)
            throw new ArgumentException("End time must be after start time.", nameof(endTimeUtc));

        return new Appointment
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            DoctorId = doctorId,
            ScheduledStartTimeUtc = startTimeUtc,
            ScheduledEndTimeUtc = endTimeUtc,
            Status = AppointmentStatus.Scheduled,
            Type = type,
            ReasonForVisit = reasonForVisit,
            QueueNumber = queueNumber
        };
    }

    public void Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException($"Cannot confirm appointment with status '{Status}'.");

        Status = AppointmentStatus.Confirmed;
        UpdateModifiedTime();
    }

    public void StartConsultation()
    {
        if (Status is not (AppointmentStatus.Scheduled or AppointmentStatus.Confirmed))
            throw new InvalidOperationException($"Cannot start appointment with status '{Status}'.");

        Status = AppointmentStatus.InProgress;
        UpdateModifiedTime();
    }

    public void Complete()
    {
        if (Status != AppointmentStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete appointment that is not InProgress.");

        Status = AppointmentStatus.Completed;
        UpdateModifiedTime();
    }

    public void Cancel(string reason)
    {
        if (Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot cancel appointment with status '{Status}'.");

        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason;
        UpdateModifiedTime();
    }

    public void Reschedule(DateTime newStartTimeUtc, DateTime newEndTimeUtc)
    {
        if (newEndTimeUtc <= newStartTimeUtc)
            throw new ArgumentException("End time must be after start time.", nameof(newEndTimeUtc));

        if (Status is AppointmentStatus.Completed or AppointmentStatus.Cancelled)
            throw new InvalidOperationException($"Cannot reschedule an appointment that is already {Status}.");

        ScheduledStartTimeUtc = newStartTimeUtc;
        ScheduledEndTimeUtc = newEndTimeUtc;
        Status = AppointmentStatus.Scheduled;
        UpdateModifiedTime();
    }
}
