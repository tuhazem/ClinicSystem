using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Appointments;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Infrastructure.Persistence.Repositories;

public class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _context;

    public AppointmentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetByDoctorIdAsync(Guid doctorId, DateTime dateUtc, CancellationToken cancellationToken = default)
    {
        var startOfDay = dateUtc.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.ScheduledStartTimeUtc >= startOfDay && a.ScheduledStartTimeUtc < endOfDay)
            .OrderBy(a => a.ScheduledStartTimeUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetByPatientIdAsync(Guid patientId, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.PatientId == patientId)
            .OrderByDescending(a => a.ScheduledStartTimeUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetNextQueueNumberAsync(Guid doctorId, DateTime dateUtc, CancellationToken cancellationToken = default)
    {
        var startOfDay = dateUtc.Date;
        var endOfDay = startOfDay.AddDays(1);

        var count = await _context.Appointments
            .CountAsync(a => a.DoctorId == doctorId && a.ScheduledStartTimeUtc >= startOfDay && a.ScheduledStartTimeUtc < endOfDay, cancellationToken);

        return count + 1;
    }

    public async Task<bool> HasConflictAsync(Guid doctorId, DateTime startTimeUtc, DateTime endTimeUtc, Guid? excludeAppointmentId = null, CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.Status != AppointmentStatus.Cancelled)
            .Where(a => excludeAppointmentId == null || a.Id != excludeAppointmentId)
            .AnyAsync(a => a.ScheduledStartTimeUtc < endTimeUtc && a.ScheduledEndTimeUtc > startTimeUtc, cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        await _context.Appointments.AddAsync(appointment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Appointment appointment, CancellationToken cancellationToken = default)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
