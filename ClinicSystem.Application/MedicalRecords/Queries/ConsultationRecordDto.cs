using System;
using System.Collections.Generic;

namespace ClinicSystem.Application.MedicalRecords.Queries;

public record PrescriptionItemDto(
    Guid Id,
    string MedicationName,
    string Dosage,
    string Frequency,
    int DurationInDays,
    string? Instructions
);

public record ConsultationRecordDto(
    Guid Id,
    Guid PatientId,
    Guid DoctorId,
    Guid? AppointmentId,
    string Symptoms,
    string Diagnosis,
    string? TreatmentPlan,
    string? Notes,
    DateTime CreatedAtUtc,
    IReadOnlyList<PrescriptionItemDto> Prescriptions
);
