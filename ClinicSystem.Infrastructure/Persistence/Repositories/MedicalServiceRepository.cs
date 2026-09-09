using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Infrastructure.Persistence.Repositories;

public class MedicalServiceRepository : IMedicalServiceRepository
{
    private readonly ApplicationDbContext _context;

    public MedicalServiceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MedicalService?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.MedicalServices.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<MedicalService?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var normalizedCode = code.Trim().ToUpperInvariant();
        return await _context.MedicalServices.FirstOrDefaultAsync(s => s.Code == normalizedCode, cancellationToken);
    }

    public async Task<IReadOnlyList<MedicalService>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        IQueryable<MedicalService> query = _context.MedicalServices;
        if (activeOnly)
        {
            query = query.Where(s => s.IsActive);
        }

        return await query.OrderBy(s => s.Category).ThenBy(s => s.Name).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(MedicalService service, CancellationToken cancellationToken = default)
    {
        await _context.MedicalServices.AddAsync(service, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
