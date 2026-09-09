using ClinicSystem.Domain.Common;
using System;

namespace ClinicSystem.Domain.Doctors;

public class Doctor : BaseEntity
{
    public string FullName { get; private set; } = null!;
    public Specialization Specialization { get; private set; }
    public string LicenseNumber { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;
    public string? Email { get; private set; }
    public decimal ConsultationFee { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Doctor() { }

    public static Doctor Create(
        string fullName,
        Specialization specialization,
        string licenseNumber,
        string phoneNumber,
        decimal consultationFee,
        string? email = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Doctor's full name is required.", nameof(fullName));

        if (string.IsNullOrWhiteSpace(licenseNumber))
            throw new ArgumentException("License number is required.", nameof(licenseNumber));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

        if (consultationFee < 0)
            throw new ArgumentException("Consultation fee cannot be negative.", nameof(consultationFee));

        return new Doctor
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Specialization = specialization,
            LicenseNumber = licenseNumber,
            PhoneNumber = phoneNumber,
            ConsultationFee = consultationFee,
            Email = email,
            IsActive = true
        };
    }

    public void UpdateProfile(string fullName, Specialization specialization, string phoneNumber, decimal consultationFee, string? email)
    {
        FullName = fullName;
        Specialization = specialization;
        PhoneNumber = phoneNumber;
        ConsultationFee = consultationFee;
        Email = email;
        UpdateModifiedTime();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateModifiedTime();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateModifiedTime();
    }
}
