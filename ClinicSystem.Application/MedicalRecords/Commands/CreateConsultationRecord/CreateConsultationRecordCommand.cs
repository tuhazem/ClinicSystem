using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.MedicalRecords;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.MedicalRecords.Commands.CreateConsultationRecord;

public record PrescriptionItemInput(
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationInDays,
    string? Instructions = null
);

public record CreateConsultationRecordCommand(
    Guid PatientId,
    Guid DoctorId,
    string Symptoms,
    string Diagnosis,
    string? TreatmentPlan = null,
    string? Notes = null,
    Guid? AppointmentId = null,
    List<PrescriptionItemInput>? Prescriptions = null
) : IRequest<Guid>, ICacheInvalidator
{
    public IReadOnlyCollection<string>? CacheTagsToInvalidate => ["consultations", $"patient-{PatientId}"];
}

public class CreateConsultationRecordCommandValidator : AbstractValidator<CreateConsultationRecordCommand>
{
    public CreateConsultationRecordCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("Patient ID is required.");
        RuleFor(x => x.DoctorId).NotEmpty().WithMessage("Doctor ID is required.");
        RuleFor(x => x.Symptoms).NotEmpty().WithMessage("Symptoms are required.");
        RuleFor(x => x.Diagnosis).NotEmpty().WithMessage("Diagnosis is required.");
    }
}

public class CreateConsultationRecordCommandHandler(IConsultationRepository consultationRepository)
    : IRequestHandler<CreateConsultationRecordCommand, Guid>
{
    public async Task<Guid> Handle(CreateConsultationRecordCommand request, CancellationToken cancellationToken)
    {
        var record = ConsultationRecord.Create(
            request.PatientId,
            request.DoctorId,
            request.Symptoms,
            request.Diagnosis,
            request.TreatmentPlan,
            request.Notes,
            request.AppointmentId
        );

        if (request.Prescriptions is { Count: > 0 })
        {
            foreach (var p in request.Prescriptions)
            {
                record.AddPrescription(p.MedicationName, p.Dosage, p.Frequency, p.DurationInDays, p.Instructions);
            }
        }

        await consultationRepository.AddAsync(record, cancellationToken);
        return record.Id;
    }
}
