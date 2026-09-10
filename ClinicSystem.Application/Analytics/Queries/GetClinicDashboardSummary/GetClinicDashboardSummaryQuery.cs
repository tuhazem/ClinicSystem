using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Analytics.Queries.GetClinicDashboardSummary;

public record GetClinicDashboardSummaryQuery(DateTime? DateUtc = null)
    : IRequest<DashboardSummaryDto>, ICachableQuery<DashboardSummaryDto>
{
    public string CacheKey => $"dashboard-summary-{(DateUtc ?? DateTime.UtcNow).Date:yyyyMMdd}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(1);
    public IReadOnlyCollection<string>? Tags => ["dashboard", "appointments", "invoices"];
}

public class GetClinicDashboardSummaryQueryHandler(IClinicAnalyticsService analyticsService)
    : IRequestHandler<GetClinicDashboardSummaryQuery, DashboardSummaryDto>
{
    public async Task<DashboardSummaryDto> Handle(
        GetClinicDashboardSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var targetDate = request.DateUtc ?? DateTime.UtcNow;
        return await analyticsService.GetDashboardSummaryAsync(targetDate, cancellationToken);
    }
}
