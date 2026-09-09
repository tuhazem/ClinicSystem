using ClinicSystem.Domain.Common;
using System;

namespace ClinicSystem.Domain.MedicalRecords;

public class PrescriptionItem : BaseEntity
{
    public Guid ConsultationRecordId { get; private set; }
    public string MedicationName { get; private set; } = null!;
    public string Dosage { get; private set; } = null!;
    public string Frequency { get; private set; } = null!;
    public int DurationInDays { get; private set; }
    public string? Instructions { get; private set; }

    private PrescriptionItem() { }

    public static PrescriptionItem Create(
        string medicationName,
        string dosage,
        string frequency,
        int durationInDays,
        string? instructions = null)
    {
        if (string.IsNullOrWhiteSpace(medicationName))
            throw new ArgumentException("Medication name is required.", nameof(medicationName));

        if (string.IsNullOrWhiteSpace(dosage))
            throw new ArgumentException("Dosage is required.", nameof(dosage));

        if (string.IsNullOrWhiteSpace(frequency))
            throw new ArgumentException("Frequency is required.", nameof(frequency));

        if (durationInDays <= 0)
            throw new ArgumentException("Duration in days must be greater than zero.", nameof(durationInDays));

        return new PrescriptionItem
        {
            Id = Guid.NewGuid(),
            MedicationName = medicationName.Trim(),
            Dosage = dosage.Trim(),
            Frequency = frequency.Trim(),
            DurationInDays = durationInDays,
            Instructions = instructions?.Trim()
        };
    }
}
