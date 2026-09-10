using ClinicSystem.Domain.Billing;
using ClinicSystem.Domain.Doctors;
using ClinicSystem.Domain.MedicalRecords;
using ClinicSystem.Domain.Patients;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Common.Interfaces;

public interface IPdfReportService
{
    Task<byte[]> GenerateInvoicePdfAsync(Invoice invoice, Patient patient, Doctor? doctor, CancellationToken cancellationToken = default);
    Task<byte[]> GeneratePrescriptionPdfAsync(ConsultationRecord consultation, Patient patient, Doctor doctor, CancellationToken cancellationToken = default);
}
