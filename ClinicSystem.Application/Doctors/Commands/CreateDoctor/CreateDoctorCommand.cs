using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Doctors;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Doctors.Commands.CreateDoctor;

public record CreateDoctorCommand(
    string FullName,
    Specialization Specialization,
    string LicenseNumber,
    string PhoneNumber,
    decimal ConsultationFee,
    string? Email = null
) : IRequest<Guid>, ICacheInvalidator
{
    public IReadOnlyCollection<string>? CacheTagsToInvalidate => ["doctors"];
}

public class CreateDoctorCommandValidator : AbstractValidator<CreateDoctorCommand>
{
    public CreateDoctorCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(150);

        RuleFor(x => x.LicenseNumber)
            .NotEmpty().WithMessage("License number is required.")
            .MaximumLength(50);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Phone number is required.")
            .MaximumLength(20);

        RuleFor(x => x.ConsultationFee)
            .GreaterThanOrEqualTo(0).WithMessage("Consultation fee cannot be negative.");
    }
}

public class CreateDoctorCommandHandler(IDoctorRepository doctorRepository) : IRequestHandler<CreateDoctorCommand, Guid>
{
    public async Task<Guid> Handle(CreateDoctorCommand request, CancellationToken cancellationToken)
    {
        var doctor = Doctor.Create(
            request.FullName,
            request.Specialization,
            request.LicenseNumber,
            request.PhoneNumber,
            request.ConsultationFee,
            request.Email
        );

        await doctorRepository.AddAsync(doctor, cancellationToken);
        return doctor.Id;
    }
}
