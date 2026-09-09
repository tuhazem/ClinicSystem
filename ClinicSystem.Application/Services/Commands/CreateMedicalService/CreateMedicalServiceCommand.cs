using ClinicSystem.Application.Common.Caching;
using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Services;
using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Services.Commands.CreateMedicalService;

public record CreateMedicalServiceCommand(
    string Code,
    string Name,
    decimal BasePrice,
    string Category,
    string? Description = null
) : IRequest<Guid>, ICacheInvalidator
{
    public IReadOnlyCollection<string>? CacheTagsToInvalidate => ["services"];
}

public class CreateMedicalServiceCommandValidator : AbstractValidator<CreateMedicalServiceCommand>
{
    public CreateMedicalServiceCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Service code is required.")
            .MaximumLength(20);

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Service name is required.")
            .MaximumLength(150);

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0).WithMessage("Base price cannot be negative.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .MaximumLength(100);
    }
}

public class CreateMedicalServiceCommandHandler(IMedicalServiceRepository serviceRepository)
    : IRequestHandler<CreateMedicalServiceCommand, Guid>
{
    public async Task<Guid> Handle(CreateMedicalServiceCommand request, CancellationToken cancellationToken)
    {
        var service = MedicalService.Create(
            request.Code,
            request.Name,
            request.BasePrice,
            request.Category,
            request.Description
        );

        await serviceRepository.AddAsync(service, cancellationToken);
        return service.Id;
    }
}
