using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Billing;
using ClinicSystem.Domain.Doctors;
using ClinicSystem.Domain.MedicalRecords;
using ClinicSystem.Domain.Patients;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Infrastructure.Services;

public class PdfReportService : IPdfReportService
{
    static PdfReportService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateInvoicePdfAsync(
        Invoice invoice,
        Patient patient,
        Doctor? doctor,
        CancellationToken cancellationToken = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                // Header
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("HEALTHCARE CLINIC").FontSize(20).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text("100 Medical Plaza, Suite 400").FontColor(Colors.Grey.Darken1);
                        col.Item().Text("Phone: +1 (555) 019-2834 | Email: billing@clinic.com").FontColor(Colors.Grey.Darken1);
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("INVOICE / RECEIPT").FontSize(18).Bold().FontColor(Colors.Grey.Darken3);
                        col.Item().Text($"Invoice #: {invoice.InvoiceNumber}").Bold();
                        col.Item().Text($"Date: {invoice.CreatedAtUtc:yyyy-MM-dd HH:mm} UTC");
                        col.Item().Text($"Status: {invoice.Status.ToString().ToUpperInvariant()}")
                            .Bold()
                            .FontColor(invoice.Status == InvoiceStatus.Paid ? Colors.Green.Darken2 : Colors.Orange.Darken2);
                    });
                });

                // Content
                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Spacing(15);

                    // Patient & Doctor Information
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                        {
                            c.Item().Text("BILLED TO:").Bold().FontColor(Colors.Blue.Darken2);
                            c.Item().Text(patient.FullName).Bold();
                            c.Item().Text($"Phone: {patient.ContactInfo.PhoneNumber}");
                            if (!string.IsNullOrWhiteSpace(patient.ContactInfo.Email))
                                c.Item().Text($"Email: {patient.ContactInfo.Email}");
                            if (!string.IsNullOrWhiteSpace(patient.ContactInfo.Address))
                                c.Item().Text($"Address: {patient.ContactInfo.Address}");
                        });

                        row.Spacing(15);

                        row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                        {
                            c.Item().Text("ATTENDING DOCTOR:").Bold().FontColor(Colors.Blue.Darken2);
                            if (doctor != null)
                            {
                                c.Item().Text(doctor.FullName).Bold();
                                c.Item().Text($"Specialization: {doctor.Specialization}");
                                c.Item().Text($"License: {doctor.LicenseNumber}");
                            }
                            else
                            {
                                c.Item().Text("General Clinic Services");
                            }
                        });
                    });

                    // Items Table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Description").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Qty").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Unit Price").Bold().FontColor(Colors.White);
                            header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignRight().Text("Total").Bold().FontColor(Colors.White);
                        });

                        foreach (var item in invoice.Items)
                        {
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Description);
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text(item.Quantity.ToString());
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"${item.UnitPrice:N2}");
                            table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).AlignRight().Text($"${item.TotalPrice:N2}");
                        }
                    });

                    // Summary & Totals
                    col.Item().AlignRight().Width(220).Column(c =>
                    {
                        c.Spacing(4);
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Total Amount:").Bold();
                            r.RelativeItem().AlignRight().Text($"${invoice.TotalAmount:N2}").Bold();
                        });
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Amount Paid:").FontColor(Colors.Green.Darken2);
                            r.RelativeItem().AlignRight().Text($"${invoice.PaidAmount:N2}").FontColor(Colors.Green.Darken2);
                        });
                        c.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Balance Due:").Bold().FontSize(12).FontColor(invoice.BalanceDue > 0 ? Colors.Red.Darken2 : Colors.Grey.Darken2);
                            r.RelativeItem().AlignRight().Text($"${invoice.BalanceDue:N2}").Bold().FontSize(12).FontColor(invoice.BalanceDue > 0 ? Colors.Red.Darken2 : Colors.Grey.Darken2);
                        });
                    });
                });

                // Footer
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Thank you for choosing Healthcare Clinic. Generated automatically on ");
                    x.Span($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
                });
            });
        });

        using var memoryStream = new MemoryStream();
        document.GeneratePdf(memoryStream);
        return Task.FromResult(memoryStream.ToArray());
    }

    public Task<byte[]> GeneratePrescriptionPdfAsync(
        ConsultationRecord consultation,
        Patient patient,
        Doctor doctor,
        CancellationToken cancellationToken = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                // Header
                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("HEALTHCARE MEDICAL CENTER").FontSize(18).Bold().FontColor(Colors.Teal.Darken2);
                        col.Item().Text("Department of Clinical Practice").FontSize(11).FontColor(Colors.Grey.Darken2);
                        col.Item().Text("100 Medical Plaza, Suite 400 | Phone: +1 (555) 019-2834").FontColor(Colors.Grey.Darken1);
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text($"Dr. {doctor.FullName}").FontSize(14).Bold().FontColor(Colors.Grey.Darken3);
                        col.Item().Text($"{doctor.Specialization} Specialist").Italic();
                        col.Item().Text($"License: {doctor.LicenseNumber}");
                        col.Item().Text($"Date: {consultation.CreatedAtUtc:yyyy-MM-dd}");
                    });
                });

                // Content
                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Spacing(15);

                    // Patient Details Box
                    col.Item().Border(1).BorderColor(Colors.Teal.Lighten3).Background(Colors.Teal.Lighten5).Padding(10).Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Patient Name: ").Bold(); t.Span(patient.FullName); });
                            c.Item().Text(t => { t.Span("Phone: ").Bold(); t.Span(patient.ContactInfo.PhoneNumber); });
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text(t => { t.Span("Date of Birth: ").Bold(); t.Span($"{patient.DateOfBirth:yyyy-MM-dd}"); });
                            if (!string.IsNullOrWhiteSpace(patient.ContactInfo.Email))
                                c.Item().Text(t => { t.Span("Email: ").Bold(); t.Span(patient.ContactInfo.Email); });
                        });
                    });

                    // Clinical Assessment Box
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                    {
                        c.Spacing(5);
                        c.Item().Text("CLINICAL ASSESSMENT").Bold().FontColor(Colors.Teal.Darken2);
                        c.Item().Text(t => { t.Span("Presenting Symptoms: ").Bold(); t.Span(consultation.Symptoms); });
                        c.Item().Text(t => { t.Span("Primary Diagnosis: ").Bold(); t.Span(consultation.Diagnosis); });
                        if (!string.IsNullOrWhiteSpace(consultation.TreatmentPlan))
                            c.Item().Text(t => { t.Span("Treatment Plan: ").Bold(); t.Span(consultation.TreatmentPlan); });
                        if (!string.IsNullOrWhiteSpace(consultation.Notes))
                            c.Item().Text(t => { t.Span("Clinical Notes: ").Bold(); t.Span(consultation.Notes); });
                    });

                    // Prescriptions (Rx) Section
                    col.Item().Column(c =>
                    {
                        c.Spacing(8);
                        c.Item().Row(r =>
                        {
                            r.RelativeItem().Text("PRESCRIPTION (Rx)").FontSize(14).Bold().FontColor(Colors.Teal.Darken2);
                        });

                        c.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(3);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("Medication").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("Dosage").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("Frequency").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("Days").Bold().FontColor(Colors.White);
                                header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("Instructions").Bold().FontColor(Colors.White);
                            });

                            foreach (var item in consultation.Prescriptions)
                            {
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.MedicationName).Bold();
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Dosage);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Frequency);
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text($"{item.DurationInDays} d");
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(item.Instructions ?? "-");
                            }
                        });
                    });

                    // Doctor Signature Box
                    col.Item().PaddingTop(25).Row(row =>
                    {
                        row.RelativeItem();
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                            c.Item().AlignCenter().Text($"Dr. {doctor.FullName}").Bold();
                            c.Item().AlignCenter().Text($"Medical License: {doctor.LicenseNumber}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            c.Item().AlignCenter().Text("Authorized Signature").FontSize(8).Italic();
                        });
                    });
                });

                // Footer
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Prescription generated securely by Healthcare Clinic System | ");
                    x.Span($"Record ID: {consultation.Id}");
                });
            });
        });

        using var memoryStream = new MemoryStream();
        document.GeneratePdf(memoryStream);
        return Task.FromResult(memoryStream.ToArray());
    }
}
