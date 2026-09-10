using ClinicSystem.Application.Billing.Commands.RecordPayment;
using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Billing;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ClinicSystem.UnitTests.Application;

public class RecordPaymentCommandHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock = new();
    private readonly RecordPaymentCommandHandler _handler;

    public RecordPaymentCommandHandlerTests()
    {
        _handler = new RecordPaymentCommandHandler(_invoiceRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenInvoiceExists_ShouldRecordPaymentAndReturnTrue()
    {
        // Arrange
        var invoice = Invoice.Create("INV-2026-0001", Guid.NewGuid());
        invoice.AddItem("Medical Service", 1, 100.00m);
        invoice.Issue();

        var command = new RecordPaymentCommand(invoice.Id, 100.00m, PaymentMethod.CreditCard);

        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(command.InvoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.PaidAmount.Should().Be(100.00m);
        _invoiceRepositoryMock.Verify(r => r.UpdateAsync(invoice, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenInvoiceNotFound_ShouldReturnFalse()
    {
        // Arrange
        var command = new RecordPaymentCommand(Guid.NewGuid(), 50.00m, PaymentMethod.Cash);

        _invoiceRepositoryMock.Setup(r => r.GetByIdAsync(command.InvoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();
        _invoiceRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Invoice>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
