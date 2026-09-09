using ClinicSystem.Domain.Doctors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicSystem.Infrastructure.Persistence.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.ToTable("Doctors");

        builder.HasKey(d => d.Id);
        builder.Ignore(d => d.DomainEvents);

        builder.Property(d => d.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(d => d.LicenseNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(d => d.LicenseNumber)
            .IsUnique();

        builder.Property(d => d.PhoneNumber)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.Email)
            .HasMaxLength(100);

        builder.Property(d => d.ConsultationFee)
            .HasPrecision(18, 2)
            .IsRequired();
    }
}
