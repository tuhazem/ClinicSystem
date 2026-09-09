using System;

namespace ClinicSystem.Application.Patients.Queries.GetPatientById
{
    public record PatientDto(
        Guid Id,
        string FullName,
        string MedicalRecordNumber,
        DateTime DateOfBirth,
        string PhoneNumber,
        string? Email,
        string? Address,
        string? MedicalHistory,
        DateTime CreatedAtUtc,
        DateTime? LastModifiedAtUtc
    );
}
