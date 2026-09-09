using ClinicSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClinicSystem.Domain.Billing;

public class Invoice : BaseEntity
{
    private readonly List<InvoiceItem> _items = new();

    public string InvoiceNumber { get; private set; } = null!;
    public Guid PatientId { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public InvoiceStatus Status { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }
    public decimal TotalAmount => _items.Sum(i => i.TotalPrice);
    public decimal PaidAmount { get; private set; }
    public decimal BalanceDue => TotalAmount - PaidAmount;
    public DateTime? PaidAtUtc { get; private set; }
    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    private Invoice() { }

    public static Invoice Create(
        string invoiceNumber,
        Guid patientId,
        Guid? appointmentId = null)
    {
        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new ArgumentException("Invoice number is required.", nameof(invoiceNumber));

        if (patientId == Guid.Empty)
            throw new ArgumentException("Patient ID is required.", nameof(patientId));

        return new Invoice
        {
            Id = Guid.NewGuid(),
            InvoiceNumber = invoiceNumber,
            PatientId = patientId,
            AppointmentId = appointmentId,
            Status = InvoiceStatus.Draft,
            PaidAmount = 0
        };
    }

    public void AddItem(string description, int quantity, decimal unitPrice)
    {
        if (Status is InvoiceStatus.Paid or InvoiceStatus.Cancelled)
            throw new InvalidOperationException($"Cannot add items to invoice in '{Status}' status.");

        var item = InvoiceItem.Create(description, quantity, unitPrice);
        _items.Add(item);
        UpdateModifiedTime();
    }

    public void Issue()
    {
        if (!_items.Any())
            throw new InvalidOperationException("Cannot issue an invoice with no line items.");

        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException($"Invoice already {Status}.");

        Status = InvoiceStatus.Issued;
        UpdateModifiedTime();
    }

    public void RecordPayment(decimal amount, PaymentMethod method)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

        if (Status is InvoiceStatus.Draft or InvoiceStatus.Cancelled)
            throw new InvalidOperationException($"Cannot record payment for invoice in '{Status}' status.");

        PaidAmount += amount;
        PaymentMethod = method;

        if (PaidAmount >= TotalAmount)
        {
            Status = InvoiceStatus.Paid;
            PaidAtUtc = DateTime.UtcNow;
        }
        else
        {
            Status = InvoiceStatus.PartiallyPaid;
        }

        UpdateModifiedTime();
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Cannot cancel a paid invoice.");

        Status = InvoiceStatus.Cancelled;
        UpdateModifiedTime();
    }
}
