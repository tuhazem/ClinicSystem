using ClinicSystem.Domain.Patients;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClinicSystem.Infrastructure.Persistence.Configurations;
public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(p => p.MedicalRecordNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(p => p.MedicalRecordNumber)
            .IsUnique();

        // Mapping the Value Object (PatientContactInfo) as Owned Entity
        builder.OwnsOne(p => p.ContactInfo, contact =>
        {
            contact.Property(c => c.PhoneNumber)
                .HasColumnName("PhoneNumber")
                .HasMaxLength(20)
                .IsRequired();

            contact.Property(c => c.Email)
                .HasColumnName("Email")
                .HasMaxLength(100);

            contact.Property(c => c.Address)
                .HasColumnName("Address")
                .HasMaxLength(250);
        });
    }
} 
