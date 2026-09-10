using ClinicSystem.Application.Common.Interfaces;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.MedicalRecords.Queries.GetPrescriptionPdf;

public record PrescriptionPdfResult(byte[] Content, string FileName, string ContentType = "application/pdf");

public record GetPrescriptionPdfQuery(Guid ConsultationRecordId) : IRequest<PrescriptionPdfResult>;

public class GetPrescriptionPdfQueryHandler(
    IConsultationRepository consultationRepository,
    IPatientRepository patientRepository,
    IDoctorRepository doctorRepository,
    IPdfReportService pdfReportService)
    : IRequestHandler<GetPrescriptionPdfQuery, PrescriptionPdfResult>
{
    public async Task<PrescriptionPdfResult> Handle(GetPrescriptionPdfQuery request, CancellationToken cancellationToken)
    {
        var record = await consultationRepository.GetByIdAsync(request.ConsultationRecordId, cancellationToken);
        if (record == null)
        {
            throw new KeyNotFoundException($"Consultation record '{request.ConsultationRecordId}' was not found.");
        }

        var patient = await patientRepository.GetByIdAsync(record.PatientId, cancellationToken);
        if (patient == null)
        {
            throw new KeyNotFoundException($"Patient '{record.PatientId}' was not found.");
        }

        var doctor = await doctorRepository.GetByIdAsync(record.DoctorId, cancellationToken);
        if (doctor == null)
        {
            throw new KeyNotFoundException($"Doctor '{record.DoctorId}' was not found.");
        }

        var pdfBytes = await pdfReportService.GeneratePrescriptionPdfAsync(record, patient, doctor, cancellationToken);
        var fileName = $"Prescription_{patient.FullName.Replace(" ", "_")}_{record.CreatedAtUtc:yyyyMMdd}.pdf";

        return new PrescriptionPdfResult(pdfBytes, fileName);
    }
}
