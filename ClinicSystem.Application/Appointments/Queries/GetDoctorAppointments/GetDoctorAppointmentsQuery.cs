using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Appointments.Queries.GetDoctorAppointments;

public record GetDoctorAppointmentsQuery(Guid DoctorId, DateTime DateUtc)
    : IRequest<IReadOnlyList<AppointmentDto>>, ICachableQuery<IReadOnlyList<AppointmentDto>>
{
    public string CacheKey => $"appointments:doctor:{DoctorId}:date:{DateUtc:yyyy-MM-dd}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(3);
    public IReadOnlyCollection<string>? Tags => ["appointments", $"doctor-{DoctorId}-appointments"];
}

public class GetDoctorAppointmentsQueryHandler(IAppointmentRepository appointmentRepository)
    : IRequestHandler<GetDoctorAppointmentsQuery, IReadOnlyList<AppointmentDto>>
{
    public async Task<IReadOnlyList<AppointmentDto>> Handle(GetDoctorAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var appointments = await appointmentRepository.GetByDoctorIdAsync(request.DoctorId, request.DateUtc, cancellationToken);
        return appointments.Select(a => new AppointmentDto(
            a.Id,
            a.PatientId,
            a.DoctorId,
            a.ScheduledStartTimeUtc,
            a.ScheduledEndTimeUtc,
            a.Status,
            a.Status.ToString(),
            a.Type,
            a.Type.ToString(),
            a.ReasonForVisit,
            a.CancellationReason,
            a.QueueNumber
        )).ToList();
    }
}
