namespace ClinicSystem.Domain.Billing;

public enum InvoiceStatus
{
    Draft = 1,
    Issued = 2,
    Paid = 3,
    PartiallyPaid = 4,
    Cancelled = 5
}
