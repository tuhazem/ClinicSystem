using ClinicSystem.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSystem.Domain.Patients.ValueObjects
{
    public class PatientContactInfo : ValueObject
    {
        public string PhoneNumber { get; }
        public string? Email { get; }
        public string? Address { get; }

        private PatientContactInfo(string phoneNumber, string? email, string? address)
        {
            PhoneNumber = phoneNumber;
            Email = email;
            Address = address;
        }


        public static PatientContactInfo Create(string phoneNumber, string? email = null, string? address = null)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
                throw new ArgumentException("Phone number is required.", nameof(phoneNumber));

            return new PatientContactInfo(phoneNumber, email, address);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return PhoneNumber;
            yield return Email ?? string.Empty;
            yield return Address ?? string.Empty;
        }
    }
}
