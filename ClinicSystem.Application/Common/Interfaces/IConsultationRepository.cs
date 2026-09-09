using ClinicSystem.Domain.MedicalRecords;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Common.Interfaces;

public interface IConsultationRepository
{
    Task<ConsultationRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ConsultationRecord>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default);
    Task AddAsync(ConsultationRecord consultation, CancellationToken cancellationToken = default);
}
