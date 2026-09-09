using ClinicSystem.Application.Common.Caching;
using MediatR;
using System;
using System.Collections.Generic;

namespace ClinicSystem.Application.Patients.Queries.GetPatientById;

public record GetPatientByIdQuery(Guid Id) : IRequest<PatientDto?>, ICachableQuery<PatientDto?>
{
    public string CacheKey => $"patient:{Id}";

    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);

    public IReadOnlyCollection<string>? Tags => ["patients", $"patient-{Id}"];
}
