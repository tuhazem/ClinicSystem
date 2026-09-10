using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Common.Interfaces;

public record AppointmentsSummaryDto(
    int TotalToday,
    int Scheduled,
    int Confirmed,
    int InProgress,
    int Completed,
    int Cancelled
);

public record FinancialSummaryDto(
    decimal TodayRevenue,
    decimal MonthRevenue,
    decimal TotalOutstandingBalance,
    int UnpaidInvoicesCount
);

public record TopServiceSummaryDto(
    string ServiceName,
    string Category,
    int UsageCount,
    decimal TotalRevenue
);

public record DashboardSummaryDto(
    AppointmentsSummaryDto Appointments,
    FinancialSummaryDto Financials,
    int TotalActivePatients,
    int TotalActiveDoctors,
    IReadOnlyList<TopServiceSummaryDto> TopServices
);

public interface IClinicAnalyticsService
{
    Task<DashboardSummaryDto> GetDashboardSummaryAsync(DateTime dateUtc, CancellationToken cancellationToken = default);
}
