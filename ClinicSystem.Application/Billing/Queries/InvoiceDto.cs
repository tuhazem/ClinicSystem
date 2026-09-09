using ClinicSystem.Domain.Billing;
using System;
using System.Collections.Generic;

namespace ClinicSystem.Application.Billing.Queries;

public record InvoiceItemDto(
    Guid Id,
    string Description,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice
);

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid PatientId,
    Guid? AppointmentId,
    InvoiceStatus Status,
    string StatusName,
    PaymentMethod? PaymentMethod,
    string? PaymentMethodName,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal BalanceDue,
    DateTime? PaidAtUtc,
    DateTime CreatedAtUtc,
    IReadOnlyList<InvoiceItemDto> Items
);
