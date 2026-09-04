using ClinicSystem.Domain.Patients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Common.Interfaces
{
    public interface IPatientRepository
    {
        Task AddAsync(Patient patient, CancellationToken cancellationToken = default);
        Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<string> GenerateNextMedicalRecordNumberAsync(CancellationToken cancellationToken = default);
    }
}
