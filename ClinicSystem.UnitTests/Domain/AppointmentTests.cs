using ClinicSystem.Domain.Appointments;
using FluentAssertions;
using System;
using Xunit;

namespace ClinicSystem.UnitTests.Domain;

public class AppointmentTests
{
    [Fact]
    public void Schedule_WithValidData_ShouldCreateScheduledAppointment()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var doctorId = Guid.NewGuid();
        var startTime = DateTime.UtcNow.AddHours(2);
        var endTime = startTime.AddMinutes(30);

        // Act
        var appointment = Appointment.Schedule(
            patientId,
            doctorId,
            startTime,
            endTime,
            AppointmentType.GeneralConsultation,
            "Fever and chills",
            1
        );

        // Assert
        appointment.Should().NotBeNull();
        appointment.PatientId.Should().Be(patientId);
        appointment.DoctorId.Should().Be(doctorId);
        appointment.ScheduledStartTimeUtc.Should().Be(startTime);
        appointment.ScheduledEndTimeUtc.Should().Be(endTime);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        appointment.Type.Should().Be(AppointmentType.GeneralConsultation);
        appointment.ReasonForVisit.Should().Be("Fever and chills");
        appointment.QueueNumber.Should().Be(1);
    }

    [Fact]
    public void Schedule_WhenEndTimeBeforeStartTime_ShouldThrowArgumentException()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddHours(2);
        var endTime = startTime.AddMinutes(-30);

        // Act
        Action act = () => Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            startTime,
            endTime,
            AppointmentType.GeneralConsultation
        );

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*End time must be after start time*");
    }

    [Fact]
    public void Lifecycle_ConfirmToInProgressToComplete_ShouldTransitionCorrectly()
    {
        // Arrange
        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(1.5),
            AppointmentType.GeneralConsultation
        );

        // Act & Assert 1: Confirm
        appointment.Confirm();
        appointment.Status.Should().Be(AppointmentStatus.Confirmed);

        // Act & Assert 2: InProgress
        appointment.StartConsultation();
        appointment.Status.Should().Be(AppointmentStatus.InProgress);

        // Act & Assert 3: Complete
        appointment.Complete();
        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void Cancel_WhenScheduled_ShouldSetCancelledStatusAndReason()
    {
        // Arrange
        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(1.5),
            AppointmentType.GeneralConsultation
        );

        // Act
        appointment.Cancel("Patient requested cancellation.");

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        appointment.CancellationReason.Should().Be("Patient requested cancellation.");
    }

    [Fact]
    public void Reschedule_WhenValid_ShouldUpdateTimesAndResetToScheduled()
    {
        // Arrange
        var appointment = Appointment.Schedule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(1.5),
            AppointmentType.GeneralConsultation
        );
        appointment.Confirm();

        var newStart = DateTime.UtcNow.AddDays(1);
        var newEnd = newStart.AddMinutes(30);

        // Act
        appointment.Reschedule(newStart, newEnd);

        // Assert
        appointment.ScheduledStartTimeUtc.Should().Be(newStart);
        appointment.ScheduledEndTimeUtc.Should().Be(newEnd);
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }
}
