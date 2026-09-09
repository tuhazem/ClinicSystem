using ClinicSystem.Domain.Appointments;
using System;

namespace ClinicSystem.Application.Appointments.Queries;

public record AppointmentDto(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledStartTimeUtc,
    DateTime ScheduledEndTimeUtc,
    AppointmentStatus Status,
    string StatusName,
    AppointmentType Type,
    string TypeName,
    string? ReasonForVisit,
    string? CancellationReason,
    int? QueueNumber
);
