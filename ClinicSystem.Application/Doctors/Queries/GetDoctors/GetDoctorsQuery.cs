using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Doctors.Queries.GetDoctors;

public record GetDoctorsQuery(bool ActiveOnly = true) : IRequest<IReadOnlyList<DoctorDto>>, ICachableQuery<IReadOnlyList<DoctorDto>>
{
    public string CacheKey => $"doctors:all:active={ActiveOnly}";
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
    public IReadOnlyCollection<string>? Tags => ["doctors"];
}

public class GetDoctorsQueryHandler(IDoctorRepository doctorRepository) : IRequestHandler<GetDoctorsQuery, IReadOnlyList<DoctorDto>>
{
    public async Task<IReadOnlyList<DoctorDto>> Handle(GetDoctorsQuery request, CancellationToken cancellationToken)
    {
        var doctors = await doctorRepository.GetAllAsync(request.ActiveOnly, cancellationToken);
        return doctors.Select(d => new DoctorDto(
            d.Id,
            d.FullName,
            d.Specialization,
            d.Specialization.ToString(),
            d.LicenseNumber,
            d.PhoneNumber,
            d.Email,
            d.ConsultationFee,
            d.IsActive
        )).ToList();
    }
}
