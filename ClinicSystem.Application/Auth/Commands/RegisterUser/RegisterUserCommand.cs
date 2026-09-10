using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Users;
using FluentValidation;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Application.Auth.Commands.RegisterUser;

public record AuthResponseDto(
    Guid UserId,
    string Username,
    string Email,
    string Role,
    string AccessToken,
    string RefreshToken,
    Guid? AssociatedDoctorId = null,
    Guid? AssociatedPatientId = null
);

public record RegisterUserCommand(
    string Username,
    string Email,
    string Password,
    string Role,
    Guid? AssociatedDoctorId = null,
    Guid? AssociatedPatientId = null
) : IRequest<AuthResponseDto>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3).MaximumLength(50);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(100);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("Password must be at least 6 characters.");
        RuleFor(x => x.Role).NotEmpty().Must(r => r is "Admin" or "Doctor" or "Receptionist" or "Cashier" or "Patient")
            .WithMessage("Role must be one of: Admin, Doctor, Receptionist, Cashier, Patient.");
    }
}

public class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenGenerator jwtTokenGenerator)
    : IRequestHandler<RegisterUserCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var exists = await userRepository.ExistsByUsernameOrEmailAsync(request.Username, request.Email, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"A user with username '{request.Username}' or email '{request.Email}' already exists.");
        }

        var passwordHash = passwordHasher.HashPassword(request.Password);
        var user = User.Create(
            request.Username,
            request.Email,
            passwordHash,
            request.Role,
            request.AssociatedDoctorId,
            request.AssociatedPatientId
        );

        var token = jwtTokenGenerator.GenerateToken(user);
        var refreshToken = jwtTokenGenerator.GenerateRefreshToken();
        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));

        await userRepository.AddAsync(user, cancellationToken);

        return new AuthResponseDto(
            user.Id,
            user.Username,
            user.Email,
            user.Role,
            token,
            refreshToken,
            user.AssociatedDoctorId,
            user.AssociatedPatientId
        );
    }
}
