using ClinicSystem.Domain.Common;
using System;
using System.Collections.Generic;

namespace ClinicSystem.Domain.MedicalRecords;

public class ConsultationRecord : BaseEntity
{
    private readonly List<PrescriptionItem> _prescriptions = new();

    public Guid PatientId { get; private set; }
    public Guid DoctorId { get; private set; }
    public Guid? AppointmentId { get; private set; }
    public string Symptoms { get; private set; } = null!;
    public string Diagnosis { get; private set; } = null!;
    public string? TreatmentPlan { get; private set; }
    public string? Notes { get; private set; }
    public IReadOnlyCollection<PrescriptionItem> Prescriptions => _prescriptions.AsReadOnly();

    private ConsultationRecord() { }

    public static ConsultationRecord Create(
        Guid patientId,
        Guid doctorId,
        string symptoms,
        string diagnosis,
        string? treatmentPlan = null,
        string? notes = null,
        Guid? appointmentId = null)
    {
        if (patientId == Guid.Empty)
            throw new ArgumentException("Patient ID is required.", nameof(patientId));

        if (doctorId == Guid.Empty)
            throw new ArgumentException("Doctor ID is required.", nameof(doctorId));

        if (string.IsNullOrWhiteSpace(symptoms))
            throw new ArgumentException("Symptoms are required.", nameof(symptoms));

        if (string.IsNullOrWhiteSpace(diagnosis))
            throw new ArgumentException("Diagnosis is required.", nameof(diagnosis));

        return new ConsultationRecord
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            DoctorId = doctorId,
            Symptoms = symptoms.Trim(),
            Diagnosis = diagnosis.Trim(),
            TreatmentPlan = treatmentPlan?.Trim(),
            Notes = notes?.Trim(),
            AppointmentId = appointmentId
        };
    }

    public void AddPrescription(string medicationName, string dosage, string frequency, int durationInDays, string? instructions = null)
    {
        var prescription = PrescriptionItem.Create(medicationName, dosage, frequency, durationInDays, instructions);
        _prescriptions.Add(prescription);
        UpdateModifiedTime();
    }
}
