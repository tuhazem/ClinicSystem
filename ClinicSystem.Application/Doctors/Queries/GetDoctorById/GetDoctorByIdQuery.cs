using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Doctors.Queries.GetDoctorById;

public record GetDoctorByIdQuery(Guid Id) : IRequest<DoctorDto?>, ICachableQuery<DoctorDto?>
{
    public string CacheKey => $"doctor:{Id}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
    public IReadOnlyCollection<string>? Tags => ["doctors", $"doctor-{Id}"];
}

public class GetDoctorByIdQueryHandler(IDoctorRepository doctorRepository) : IRequestHandler<GetDoctorByIdQuery, DoctorDto?>
{
    public async Task<DoctorDto?> Handle(GetDoctorByIdQuery request, CancellationToken cancellationToken)
    {
        var d = await doctorRepository.GetByIdAsync(request.Id, cancellationToken);
        if (d == null) return null;

        return new DoctorDto(
            d.Id,
            d.FullName,
            d.Specialization,
            d.Specialization.ToString(),
            d.LicenseNumber,
            d.PhoneNumber,
            d.Email,
            d.ConsultationFee,
            d.IsActive
        );
    }
}
