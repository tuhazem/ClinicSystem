using ClinicSystem.Domain.Common;
using ClinicSystem.Domain.Patients.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSystem.Domain.Patients
{
    public class Patient : BaseEntity
    {
        public string FullName { get; private set; } = null!;
        public string MedicalRecordNumber { get; private set; } = null!; // MRN (e.g., PAT-2026-0001)
        public DateTime DateOfBirth { get; private set; }
        public PatientContactInfo ContactInfo { get; private set; } = null!;
        public string? MedicalHistory { get; private set; }


        private Patient()
        { }

        public static Patient Register(string fullName , string mrn , DateTime dateOfBirth , PatientContactInfo contactInfo)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new ArgumentException("Full name cannot be empty.", nameof(fullName));

            if (string.IsNullOrWhiteSpace(mrn))
                throw new ArgumentException("Medical record number is required.", nameof(mrn));

            var patient = new Patient
            {
                Id = Guid.NewGuid(),
                FullName = fullName,
                MedicalRecordNumber = mrn,
                DateOfBirth = dateOfBirth,
                ContactInfo = contactInfo
            };

            return patient;
        }

        public void UpdateMedicalHistory(string newHistory)
        {
            MedicalHistory = newHistory;
            UpdateModifiedTime();
        }

    }
}
