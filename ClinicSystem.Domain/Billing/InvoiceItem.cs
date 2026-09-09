using ClinicSystem.Domain.Common;
using System;

namespace ClinicSystem.Domain.Billing;

public class InvoiceItem : BaseEntity
{
    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice => Quantity * UnitPrice;

    private InvoiceItem() { }

    public static InvoiceItem Create(string description, int quantity, decimal unitPrice)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Item description is required.", nameof(description));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        return new InvoiceItem
        {
            Id = Guid.NewGuid(),
            Description = description,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}
