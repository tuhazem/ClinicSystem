using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.MedicalRecords.Queries.GetPatientConsultations;

public record GetPatientConsultationsQuery(Guid PatientId)
    : IRequest<IReadOnlyList<ConsultationRecordDto>>, ICachableQuery<IReadOnlyList<ConsultationRecordDto>>
{
    public string CacheKey => $"consultations:patient:{PatientId}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public IReadOnlyCollection<string>? Tags => ["consultations", $"patient-{PatientId}"];
}

public class GetPatientConsultationsQueryHandler(IConsultationRepository consultationRepository)
    : IRequestHandler<GetPatientConsultationsQuery, IReadOnlyList<ConsultationRecordDto>>
{
    public async Task<IReadOnlyList<ConsultationRecordDto>> Handle(GetPatientConsultationsQuery request, CancellationToken cancellationToken)
    {
        var records = await consultationRepository.GetByPatientIdAsync(request.PatientId, cancellationToken);
        return records.Select(r => new ConsultationRecordDto(
            r.Id,
            r.PatientId,
            r.DoctorId,
            r.AppointmentId,
            r.Symptoms,
            r.Diagnosis,
            r.TreatmentPlan,
            r.Notes,
            r.CreatedAtUtc,
            r.Prescriptions.Select(p => new PrescriptionItemDto(
                p.Id,
                p.MedicationName,
                p.Dosage,
                p.Frequency,
                p.DurationInDays,
                p.Instructions
            )).ToList()
        )).ToList();
    }
}
