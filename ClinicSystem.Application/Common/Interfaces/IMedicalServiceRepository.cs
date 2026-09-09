using ClinicSystem.Domain.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Common.Interfaces;

public interface IMedicalServiceRepository
{
    Task<MedicalService?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MedicalService?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MedicalService>> GetAllAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task AddAsync(MedicalService service, CancellationToken cancellationToken = default);
}
