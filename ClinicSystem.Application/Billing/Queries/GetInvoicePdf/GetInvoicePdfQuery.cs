using ClinicSystem.Application.Common.Interfaces;
using MediatR;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Billing.Queries.GetInvoicePdf;

public record InvoicePdfResult(byte[] Content, string FileName, string ContentType = "application/pdf");

public record GetInvoicePdfQuery(Guid InvoiceId) : IRequest<InvoicePdfResult>;

public class GetInvoicePdfQueryHandler(
    IInvoiceRepository invoiceRepository,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IAppointmentRepository appointmentRepository,
    IPdfReportService pdfReportService)
    : IRequestHandler<GetInvoicePdfQuery, InvoicePdfResult>
{
    public async Task<InvoicePdfResult> Handle(GetInvoicePdfQuery request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, cancellationToken);
        if (invoice == null)
        {
            throw new KeyNotFoundException($"Invoice '{request.InvoiceId}' was not found.");
        }

        var patient = await patientRepository.GetByIdAsync(invoice.PatientId, cancellationToken);
        if (patient == null)
        {
            throw new KeyNotFoundException($"Patient '{invoice.PatientId}' was not found.");
        }

        ClinicSystem.Domain.Doctors.Doctor? doctor = null;
        if (invoice.AppointmentId.HasValue)
        {
            var appointment = await appointmentRepository.GetByIdAsync(invoice.AppointmentId.Value, cancellationToken);
            if (appointment != null)
            {
                doctor = await doctorRepository.GetByIdAsync(appointment.DoctorId, cancellationToken);
            }
        }

        var pdfBytes = await pdfReportService.GenerateInvoicePdfAsync(invoice, patient, doctor, cancellationToken);
        var fileName = $"Invoice_{invoice.InvoiceNumber}.pdf";

        return new InvoicePdfResult(pdfBytes, fileName);
    }
}
