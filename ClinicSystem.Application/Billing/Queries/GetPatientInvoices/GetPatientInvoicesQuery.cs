using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Billing.Queries.GetPatientInvoices;

public record GetPatientInvoicesQuery(Guid PatientId)
    : IRequest<IReadOnlyList<InvoiceDto>>, ICachableQuery<IReadOnlyList<InvoiceDto>>
{
    public string CacheKey => $"invoices:patient:{PatientId}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public IReadOnlyCollection<string>? Tags => ["invoices", $"patient-{PatientId}-invoices"];
}

public class GetPatientInvoicesQueryHandler(IInvoiceRepository invoiceRepository)
    : IRequestHandler<GetPatientInvoicesQuery, IReadOnlyList<InvoiceDto>>
{
    public async Task<IReadOnlyList<InvoiceDto>> Handle(GetPatientInvoicesQuery request, CancellationToken cancellationToken)
    {
        var invoices = await invoiceRepository.GetByPatientIdAsync(request.PatientId, cancellationToken);
        return invoices.Select(i => new InvoiceDto(
            i.Id,
            i.InvoiceNumber,
            i.PatientId,
            i.AppointmentId,
            i.Status,
            i.Status.ToString(),
            i.PaymentMethod,
            i.PaymentMethod?.ToString(),
            i.TotalAmount,
            i.PaidAmount,
            i.BalanceDue,
            i.PaidAtUtc,
            i.CreatedAtUtc,
            i.Items.Select(item => new InvoiceItemDto(
                item.Id,
                item.Description,
                item.Quantity,
                item.UnitPrice,
                item.TotalPrice
            )).ToList()
        )).ToList();
    }
}
