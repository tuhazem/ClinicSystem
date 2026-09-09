using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Appointments;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Appointments.Commands.UpdateAppointmentStatus;

public record UpdateAppointmentStatusCommand(
    Guid AppointmentId,
    AppointmentStatus NewStatus,
    string? Reason = null
) : IRequest<bool>, ICacheInvalidator
{
    public IReadOnlyCollection<string>? CacheTagsToInvalidate => ["appointments"];
}

public class UpdateAppointmentStatusCommandHandler(IAppointmentRepository appointmentRepository)
    : IRequestHandler<UpdateAppointmentStatusCommand, bool>
{
    public async Task<bool> Handle(UpdateAppointmentStatusCommand request, CancellationToken cancellationToken)
    {
        var appointment = await appointmentRepository.GetByIdAsync(request.AppointmentId, cancellationToken);
        if (appointment == null) return false;

        switch (request.NewStatus)
        {
            case AppointmentStatus.Confirmed:
                appointment.Confirm();
                break;
            case AppointmentStatus.InProgress:
                appointment.StartConsultation();
                break;
            case AppointmentStatus.Completed:
                appointment.Complete();
                break;
            case AppointmentStatus.Cancelled:
                appointment.Cancel(request.Reason ?? "Cancelled by user/staff");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(request.NewStatus), "Unsupported status transition.");
        }

        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
        return true;
    }
}
