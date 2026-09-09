using ClinicSystem.Domain.Common;
using System;

namespace ClinicSystem.Domain.Services;

public class MedicalService : BaseEntity
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal BasePrice { get; private set; }
    public string Category { get; private set; } = null!;
    public bool IsActive { get; private set; } = true;

    private MedicalService() { }

    public static MedicalService Create(string code, string name, decimal basePrice, string category, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Service code is required.", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Service name is required.", nameof(name));

        if (basePrice < 0)
            throw new ArgumentException("Base price cannot be negative.", nameof(basePrice));

        if (string.IsNullOrWhiteSpace(category))
            throw new ArgumentException("Category is required.", nameof(category));

        return new MedicalService
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            BasePrice = basePrice,
            Category = category.Trim(),
            Description = description,
            IsActive = true
        };
    }

    public void UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(newPrice));

        BasePrice = newPrice;
        UpdateModifiedTime();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateModifiedTime();
    }
}
