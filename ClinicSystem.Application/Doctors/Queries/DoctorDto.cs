using ClinicSystem.Domain.Doctors;
using System;

namespace ClinicSystem.Application.Doctors.Queries;

public record DoctorDto(
    Guid Id,
    string FullName,
    Specialization Specialization,
    string SpecializationName,
    string LicenseNumber,
    string PhoneNumber,
    string? Email,
    decimal ConsultationFee,
    bool IsActive
);
