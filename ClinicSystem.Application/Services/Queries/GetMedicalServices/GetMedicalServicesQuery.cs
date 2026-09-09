using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Services.Queries.GetMedicalServices;

public record GetMedicalServicesQuery(bool ActiveOnly = true)
    : IRequest<IReadOnlyList<MedicalServiceDto>>, ICachableQuery<IReadOnlyList<MedicalServiceDto>>
{
    public string CacheKey => $"services:all:active={ActiveOnly}";
    public TimeSpan? Expiration => TimeSpan.FromHours(1);
    public IReadOnlyCollection<string>? Tags => ["services"];
}

public class GetMedicalServicesQueryHandler(IMedicalServiceRepository serviceRepository)
    : IRequestHandler<GetMedicalServicesQuery, IReadOnlyList<MedicalServiceDto>>
{
    public async Task<IReadOnlyList<MedicalServiceDto>> Handle(GetMedicalServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await serviceRepository.GetAllAsync(request.ActiveOnly, cancellationToken);
        return services.Select(s => new MedicalServiceDto(
            s.Id,
            s.Code,
            s.Name,
            s.BasePrice,
            s.Category,
            s.Description,
            s.IsActive
        )).ToList();
    }
}
