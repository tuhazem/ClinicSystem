using ClinicSystem.Domain.MedicalRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicSystem.Infrastructure.Persistence.Configurations;

public class ConsultationRecordConfiguration : IEntityTypeConfiguration<ConsultationRecord>
{
    public void Configure(EntityTypeBuilder<ConsultationRecord> builder)
    {
        builder.ToTable("ConsultationRecords");

        builder.HasKey(c => c.Id);
        builder.Ignore(c => c.DomainEvents);

        builder.Property(c => c.Symptoms)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(c => c.Diagnosis)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(c => c.TreatmentPlan)
            .HasMaxLength(2000);

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        builder.HasIndex(c => c.PatientId);
        builder.HasIndex(c => c.DoctorId);

        builder.HasMany(c => c.Prescriptions)
            .WithOne()
            .HasForeignKey(p => p.ConsultationRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        var navigation = builder.Metadata.FindNavigation(nameof(ConsultationRecord.Prescriptions));
        navigation?.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}

public class PrescriptionItemConfiguration : IEntityTypeConfiguration<PrescriptionItem>
{
    public void Configure(EntityTypeBuilder<PrescriptionItem> builder)
    {
        builder.ToTable("PrescriptionItems");

        builder.HasKey(p => p.Id);
        builder.Ignore(p => p.DomainEvents);

        builder.Property(p => p.MedicationName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(p => p.Dosage)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Frequency)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Instructions)
            .HasMaxLength(500);
    }
}
