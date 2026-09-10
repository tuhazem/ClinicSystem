using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Appointments;
using ClinicSystem.Domain.Billing;
using ClinicSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Infrastructure.Services;

public class ClinicAnalyticsService(ApplicationDbContext context) : IClinicAnalyticsService
{
    public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(DateTime dateUtc, CancellationToken cancellationToken = default)
    {
        var startOfDay = dateUtc.Date;
        var endOfDay = startOfDay.AddDays(1);
        var startOfMonth = new DateTime(dateUtc.Year, dateUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var endOfMonth = startOfMonth.AddMonths(1);

        // 1. Appointments breakdown for today
        var todayAppointments = await context.Appointments
            .Where(a => a.ScheduledStartTimeUtc >= startOfDay && a.ScheduledStartTimeUtc < endOfDay)
            .ToListAsync(cancellationToken);

        var appointmentsSummary = new AppointmentsSummaryDto(
            TotalToday: todayAppointments.Count,
            Scheduled: todayAppointments.Count(a => a.Status == AppointmentStatus.Scheduled),
            Confirmed: todayAppointments.Count(a => a.Status == AppointmentStatus.Confirmed),
            InProgress: todayAppointments.Count(a => a.Status == AppointmentStatus.InProgress),
            Completed: todayAppointments.Count(a => a.Status == AppointmentStatus.Completed),
            Cancelled: todayAppointments.Count(a => a.Status == AppointmentStatus.Cancelled)
        );

        // 2. Financial Metrics
        var todayPaid = await context.Invoices
            .Where(i => i.CreatedAtUtc >= startOfDay && i.CreatedAtUtc < endOfDay && i.Status != InvoiceStatus.Cancelled)
            .SumAsync(i => (decimal?)i.PaidAmount, cancellationToken) ?? 0m;

        var monthPaid = await context.Invoices
            .Where(i => i.CreatedAtUtc >= startOfMonth && i.CreatedAtUtc < endOfMonth && i.Status != InvoiceStatus.Cancelled)
            .SumAsync(i => (decimal?)i.PaidAmount, cancellationToken) ?? 0m;

        var unpaidInvoices = await context.Invoices
            .Include(i => i.Items)
            .Where(i => i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.PartiallyPaid)
            .ToListAsync(cancellationToken);

        var outstandingBalance = unpaidInvoices.Sum(i => i.BalanceDue);
        var unpaidCount = unpaidInvoices.Count;

        var financialSummary = new FinancialSummaryDto(
            TodayRevenue: todayPaid,
            MonthRevenue: monthPaid,
            TotalOutstandingBalance: outstandingBalance,
            UnpaidInvoicesCount: unpaidCount
        );

        // 3. Counts
        var totalActivePatients = await context.Patients.CountAsync(cancellationToken);
        var totalActiveDoctors = await context.Doctors.CountAsync(d => d.IsActive, cancellationToken);

        // 4. Top 5 Services by usage from Invoice Items
        var topInvoiceItems = await context.InvoiceItems
            .GroupBy(item => item.Description)
            .Select(g => new
            {
                Description = g.Key,
                Count = g.Sum(x => x.Quantity),
                Total = g.Sum(x => x.Quantity * x.UnitPrice)
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(cancellationToken);

        var topServices = topInvoiceItems.Select(s => new TopServiceSummaryDto(
            ServiceName: s.Description,
            Category: "General",
            UsageCount: s.Count,
            TotalRevenue: s.Total
        )).ToList();

        return new DashboardSummaryDto(
            Appointments: appointmentsSummary,
            Financials: financialSummary,
            TotalActivePatients: totalActivePatients,
            TotalActiveDoctors: totalActiveDoctors,
            TopServices: topServices
        );
    }
}
