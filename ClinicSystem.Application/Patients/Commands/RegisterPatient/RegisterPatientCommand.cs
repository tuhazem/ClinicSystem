using ClinicSystem.Application.Common.Caching;
using MediatR;
using System;
using System.Collections.Generic;

namespace ClinicSystem.Application.Patients.Commands.RegisterPatient;

public record RegisterPatientCommand(
    string FullName,
    string PhoneNumber,
    DateTime DateOfBirth,
    string? Email = null,
    string? Address = null
) : IRequest<Guid>, ICacheInvalidator
{
    public IReadOnlyCollection<string>? CacheTagsToInvalidate => ["patients"];
}
