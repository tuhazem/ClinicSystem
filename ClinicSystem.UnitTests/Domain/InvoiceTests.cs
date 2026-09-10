using ClinicSystem.Domain.Billing;
using FluentAssertions;
using System;
using Xunit;

namespace ClinicSystem.UnitTests.Domain;

public class InvoiceTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateDraftInvoice()
    {
        // Arrange & Act
        var patientId = Guid.NewGuid();
        var invoice = Invoice.Create("INV-2026-0001", patientId);

        // Assert
        invoice.Should().NotBeNull();
        invoice.InvoiceNumber.Should().Be("INV-2026-0001");
        invoice.PatientId.Should().Be(patientId);
        invoice.Status.Should().Be(InvoiceStatus.Draft);
        invoice.TotalAmount.Should().Be(0);
        invoice.PaidAmount.Should().Be(0);
        invoice.BalanceDue.Should().Be(0);
    }

    [Fact]
    public void AddItem_ShouldRecalculateTotalAmount()
    {
        // Arrange
        var invoice = Invoice.Create("INV-2026-0001", Guid.NewGuid());

        // Act
        invoice.AddItem("General Consultation", 1, 150.00m);
        invoice.AddItem("CBC Blood Test", 2, 45.00m);

        // Assert
        invoice.Items.Should().HaveCount(2);
        invoice.TotalAmount.Should().Be(240.00m);
        invoice.BalanceDue.Should().Be(240.00m);
    }

    [Fact]
    public void Issue_WhenInvoiceHasItems_ShouldSetStatusToIssued()
    {
        // Arrange
        var invoice = Invoice.Create("INV-2026-0001", Guid.NewGuid());
        invoice.AddItem("General Consultation", 1, 150.00m);

        // Act
        invoice.Issue();

        // Assert
        invoice.Status.Should().Be(InvoiceStatus.Issued);
    }

    [Fact]
    public void Issue_WhenInvoiceHasNoItems_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var invoice = Invoice.Create("INV-2026-0001", Guid.NewGuid());

        // Act
        Action act = () => invoice.Issue();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot issue an invoice with no line items*");
    }

    [Fact]
    public void RecordPayment_WhenPartial_ShouldUpdateStatusToPartiallyPaid()
    {
        // Arrange
        var invoice = Invoice.Create("INV-2026-0001", Guid.NewGuid());
        invoice.AddItem("General Consultation", 1, 200.00m);
        invoice.Issue();

        // Act
        invoice.RecordPayment(100.00m, PaymentMethod.Cash);

        // Assert
        invoice.PaidAmount.Should().Be(100.00m);
        invoice.BalanceDue.Should().Be(100.00m);
        invoice.Status.Should().Be(InvoiceStatus.PartiallyPaid);
        invoice.PaymentMethod.Should().Be(PaymentMethod.Cash);
    }

    [Fact]
    public void RecordPayment_WhenFull_ShouldUpdateStatusToPaidAndSetPaidAtUtc()
    {
        // Arrange
        var invoice = Invoice.Create("INV-2026-0001", Guid.NewGuid());
        invoice.AddItem("General Consultation", 1, 200.00m);
        invoice.Issue();

        // Act
        invoice.RecordPayment(200.00m, PaymentMethod.CreditCard);

        // Assert
        invoice.PaidAmount.Should().Be(200.00m);
        invoice.BalanceDue.Should().Be(0);
        invoice.Status.Should().Be(InvoiceStatus.Paid);
        invoice.PaidAtUtc.Should().NotBeNull();
        invoice.PaymentMethod.Should().Be(PaymentMethod.CreditCard);
    }
}
