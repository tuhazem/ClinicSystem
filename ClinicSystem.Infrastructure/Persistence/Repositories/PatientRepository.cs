using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Patients;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSystem.Infrastructure.Persistence.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly ApplicationDbContext context;

        public PatientRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
        {
            await context.Patients.AddAsync(patient, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<string> GenerateNextMedicalRecordNumberAsync(CancellationToken cancellationToken = default)
        {
            var count = await context.Patients.CountAsync(cancellationToken);
            var currentYear = DateTime.UtcNow.Year;
            // Output format: PAT-2026-0001
            return $"PAT-{currentYear}-{(count + 1):D4}";

        }

        public async Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await context.Patients.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
    }
}
