using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Billing;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Billing.Commands.CreateInvoice;

public record InvoiceItemInput(string Description, int Quantity, decimal UnitPrice);

public record CreateInvoiceCommand(
    Guid PatientId,
    List<InvoiceItemInput> Items,
    Guid? AppointmentId = null
) : IRequest<Guid>, ICacheInvalidator
{
    public IReadOnlyCollection<string>? CacheTagsToInvalidate => ["invoices", $"patient-{PatientId}-invoices"];
}

public class CreateInvoiceCommandValidator : AbstractValidator<CreateInvoiceCommand>
{
    public CreateInvoiceCommandValidator()
    {
        RuleFor(x => x.PatientId).NotEmpty().WithMessage("Patient ID is required.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("Invoice must contain at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Description).NotEmpty().WithMessage("Item description is required.");
            item.RuleFor(i => i.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            item.RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative.");
        });
    }
}

public class CreateInvoiceCommandHandler(IInvoiceRepository invoiceRepository)
    : IRequestHandler<CreateInvoiceCommand, Guid>
{
    public async Task<Guid> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoiceNumber = await invoiceRepository.GenerateNextInvoiceNumberAsync(cancellationToken);
        var invoice = Invoice.Create(invoiceNumber, request.PatientId, request.AppointmentId);

        foreach (var item in request.Items)
        {
            invoice.AddItem(item.Description, item.Quantity, item.UnitPrice);
        }

        invoice.Issue();

        await invoiceRepository.AddAsync(invoice, cancellationToken);
        return invoice.Id;
    }
}
