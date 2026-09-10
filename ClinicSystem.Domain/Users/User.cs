using ClinicSystem.Domain.Common;
using System;

namespace ClinicSystem.Domain.Users;

public class User : BaseEntity
{
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string Role { get; private set; } = "Receptionist";
    public Guid? AssociatedDoctorId { get; private set; }
    public Guid? AssociatedPatientId { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiryTimeUtc { get; private set; }

    private User() { }

    public static User Create(
        string username,
        string email,
        string passwordHash,
        string role,
        Guid? associatedDoctorId = null,
        Guid? associatedPatientId = null)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new User
        {
            Id = Guid.NewGuid(),
            Username = username.Trim().ToLowerInvariant(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = role,
            AssociatedDoctorId = associatedDoctorId,
            AssociatedPatientId = associatedPatientId,
            IsActive = true
        };
    }

    public void SetRefreshToken(string refreshToken, DateTime expiryTimeUtc)
    {
        RefreshToken = refreshToken;
        RefreshTokenExpiryTimeUtc = expiryTimeUtc;
        UpdateModifiedTime();
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiryTimeUtc = null;
        UpdateModifiedTime();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateModifiedTime();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateModifiedTime();
    }
}
