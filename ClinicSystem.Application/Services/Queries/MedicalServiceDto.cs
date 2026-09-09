using System;

namespace ClinicSystem.Application.Services.Queries;

public record MedicalServiceDto(
    Guid Id,
    string Code,
    string Name,
    decimal BasePrice,
    string Category,
    string? Description,
    bool IsActive
);
