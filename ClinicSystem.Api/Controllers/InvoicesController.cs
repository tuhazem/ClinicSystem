using ClinicSystem.Application.Billing.Commands.CreateInvoice;
using ClinicSystem.Application.Billing.Commands.RecordPayment;
using ClinicSystem.Application.Billing.Queries;
using ClinicSystem.Application.Billing.Queries.GetPatientInvoices;
using ClinicSystem.Domain.Billing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Api.Controllers;

public class InvoicesController : ApiController
{
    /// <summary>
    /// Retrieves all invoices and billing records for a specific patient (cached via HybridCache).
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<InvoiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientInvoices(Guid patientId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetPatientInvoicesQuery(patientId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates and issues a new patient invoice with line items.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        var invoiceId = await Mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new { invoiceId });
    }

    /// <summary>
    /// Records a payment for an invoice (Cash, CreditCard, Insurance, BankTransfer).
    /// </summary>
    [HttpPost("{id:guid}/pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RecordPayment(Guid id, [FromBody] RecordPaymentRequest request, CancellationToken cancellationToken)
    {
        var success = await Mediator.Send(new RecordPaymentCommand(id, request.Amount, request.PaymentMethod), cancellationToken);
        if (!success) return NotFound(new { message = $"Invoice with ID '{id}' not found." });
        return Ok(new { message = "Payment recorded successfully." });
    }
}

public record RecordPaymentRequest(decimal Amount, PaymentMethod PaymentMethod);
