using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.MedicalRecords;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Infrastructure.Persistence.Repositories;

public class ConsultationRepository : IConsultationRepository
{
    private readonly ApplicationDbContext _context;

    public ConsultationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConsultationRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ConsultationRecords
            .Include(c => c.Prescriptions)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ConsultationRecord>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _context.ConsultationRecords
            .Include(c => c.Prescriptions)
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(ConsultationRecord consultation, CancellationToken cancellationToken = default)
    {
        await _context.ConsultationRecords.AddAsync(consultation, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
