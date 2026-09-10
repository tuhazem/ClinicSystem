using ClinicSystem.Application.Appointments.Commands.BookAppointment;
using ClinicSystem.Application.Appointments.Commands.UpdateAppointmentStatus;
using ClinicSystem.Application.Appointments.Queries;
using ClinicSystem.Application.Appointments.Queries.GetDoctorAppointments;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Api.Controllers;

public class AppointmentsController : ApiController
{
    /// <summary>
    /// Books a new appointment with queue number assignment and cache invalidation.
    /// </summary>
    [HttpPost("book")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Book([FromBody] BookAppointmentCommand command, CancellationToken cancellationToken)
    {
        var appointmentId = await Mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new { appointmentId });
    }

    /// <summary>
    /// Retrieves a doctor's schedule and appointments for a given date (cached via HybridCache).
    /// </summary>
    [HttpGet("doctor/{doctorId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDoctorSchedule(Guid doctorId, [FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        var targetDate = date ?? DateTime.UtcNow.Date;
        var result = await Mediator.Send(new GetDoctorAppointmentsQuery(doctorId, targetDate), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves available, non-overlapping booking slots for a doctor on a given date (cached via HybridCache).
    /// </summary>
    [HttpGet("doctor/{doctorId:guid}/available-slots")]
    [ProducesResponseType(typeof(IReadOnlyList<ClinicSystem.Application.Appointments.Queries.GetDoctorAvailableSlots.AvailableSlotDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailableSlots(Guid doctorId, [FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        var targetDate = date ?? DateTime.UtcNow.Date;
        var result = await Mediator.Send(new ClinicSystem.Application.Appointments.Queries.GetDoctorAvailableSlots.GetDoctorAvailableSlotsQuery(doctorId, targetDate), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Updates an appointment's status (Confirmed, InProgress, Completed, Cancelled).
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateAppointmentStatusRequest request, CancellationToken cancellationToken)
    {
        var success = await Mediator.Send(new UpdateAppointmentStatusCommand(id, request.NewStatus, request.Reason), cancellationToken);
        if (!success) return NotFound(new { message = $"Appointment '{id}' not found." });
        return Ok(new { message = "Status updated successfully." });
    }
}

public record UpdateAppointmentStatusRequest(ClinicSystem.Domain.Appointments.AppointmentStatus NewStatus, string? Reason = null);
