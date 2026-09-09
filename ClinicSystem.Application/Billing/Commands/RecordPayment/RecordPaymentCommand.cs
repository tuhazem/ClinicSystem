using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Billing;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Billing.Commands.RecordPayment;

public record RecordPaymentCommand(
    Guid InvoiceId,
    decimal Amount,
    PaymentMethod PaymentMethod
) : IRequest<bool>, ICacheInvalidator
{
    public IReadOnlyCollection<string>? CacheTagsToInvalidate => ["invoices"];
}

public class RecordPaymentCommandValidator : AbstractValidator<RecordPaymentCommand>
{
    public RecordPaymentCommandValidator()
    {
        RuleFor(x => x.InvoiceId).NotEmpty().WithMessage("Invoice ID is required.");
        RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
    }
}

public class RecordPaymentCommandHandler(IInvoiceRepository invoiceRepository)
    : IRequestHandler<RecordPaymentCommand, bool>
{
    public async Task<bool> Handle(RecordPaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice == null) return false;

        invoice.RecordPayment(request.Amount, request.PaymentMethod);
        await invoiceRepository.UpdateAsync(invoice, cancellationToken);
        return true;
    }
}
