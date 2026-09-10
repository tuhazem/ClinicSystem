using ClinicSystem.Domain.Doctors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicSystem.Infrastructure.Persistence.Configurations;

public class DoctorWorkingScheduleConfiguration : IEntityTypeConfiguration<DoctorWorkingSchedule>
{
    public void Configure(EntityTypeBuilder<DoctorWorkingSchedule> builder)
    {
        builder.ToTable("DoctorWorkingSchedules");

        builder.HasKey(s => s.Id);
        builder.Ignore(s => s.DomainEvents);

        builder.Property(s => s.StartTime)
            .IsRequired();

        builder.Property(s => s.EndTime)
            .IsRequired();

        builder.Property(s => s.SlotDurationMinutes)
            .HasDefaultValue(30)
            .IsRequired();

        builder.Property(s => s.DayOfWeek)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);
    }
}
