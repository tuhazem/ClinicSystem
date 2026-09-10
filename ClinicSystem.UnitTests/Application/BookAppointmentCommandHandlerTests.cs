using ClinicSystem.Application.Appointments.Commands.BookAppointment;
using ClinicSystem.Application.Common.Interfaces;
using ClinicSystem.Domain.Appointments;
using FluentAssertions;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ClinicSystem.UnitTests.Application;

public class BookAppointmentCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock = new();
    private readonly BookAppointmentCommandHandler _handler;

    public BookAppointmentCommandHandlerTests()
    {
        _handler = new BookAppointmentCommandHandler(_appointmentRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNoConflict_ShouldBookAppointmentAndReturnId()
    {
        // Arrange
        var command = new BookAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(2.5),
            AppointmentType.GeneralConsultation,
            "Consultation request"
        );

        _appointmentRepositoryMock.Setup(r => r.HasConflictAsync(
            command.DoctorId,
            command.ScheduledStartTimeUtc,
            command.ScheduledEndTimeUtc,
            null,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _appointmentRepositoryMock.Setup(r => r.GetNextQueueNumberAsync(
            command.DoctorId,
            command.ScheduledStartTimeUtc,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
        _appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenConflictExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var command = new BookAppointmentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(2),
            DateTime.UtcNow.AddHours(2.5),
            AppointmentType.GeneralConsultation
        );

        _appointmentRepositoryMock.Setup(r => r.HasConflictAsync(
            command.DoctorId,
            command.ScheduledStartTimeUtc,
            command.ScheduledEndTimeUtc,
            null,
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already has a booked appointment during this time window*");

        _appointmentRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
