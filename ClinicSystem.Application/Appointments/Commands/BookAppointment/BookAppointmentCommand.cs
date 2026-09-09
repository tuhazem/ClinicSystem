using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Appointments;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Appointments.Commands.BookAppointment;

public record BookAppointmentCommand(
    Guid PatientId,
    Guid DoctorId,
    DateTime ScheduledStartTimeUtc,
    DateTime ScheduledEndTimeUtc,
    AppointmentType Type,
    string? ReasonForVisit = null
) : IRequest<Guid>, ICacheInvalidator
{
    public IReadOnlyCollection<string>? CacheTagsToInvalidate => ["appointments", $"doctor-{DoctorId}-appointments"];
}

public class BookAppointmentCommandValidator : AbstractValidator<BookAppointmentCommand>
{
    public BookAppointmentCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("Patient ID is required.");
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage("Doctor ID is required.");
        RuleFor(x => x.ScheduledStartTimeUtc).GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future.");
        RuleFor(x => x.ScheduledEndTimeUtc).GreaterThan(x => x.ScheduledStartTimeUtc).WithMessage("End time must be after start time.");
    }
}

public class BookAppointmentCommandHandler(IAppointmentRepository appointmentRepository)
    : IRequestHandler<BookAppointmentCommand, Guid>
{
    public async Task<Guid> Handle(BookAppointmentCommand request, CancellationToken cancellationToken)
    {
        var nextQueueNumber = await appointmentRepository.GetNextQueueNumberAsync(
            request.DoctorId,
            request.ScheduledStartTimeUtc,
            cancellationToken);

        var appointment = Appointment.Schedule(
            request.PatientId,
            request.DoctorId,
            request.ScheduledStartTimeUtc,
            request.ScheduledEndTimeUtc,
            request.Type,
            request.ReasonForVisit,
            nextQueueNumber
        );

        await appointmentRepository.AddAsync(appointment, cancellationToken);
        return appointment.Id;
    }
}
