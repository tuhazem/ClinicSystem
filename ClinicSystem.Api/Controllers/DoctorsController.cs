using ClinicSystem.Application.Doctors.Commands.CreateDoctor;
using ClinicSystem.Application.Doctors.Queries;
using ClinicSystem.Application.Doctors.Queries.GetDoctorById;
using ClinicSystem.Application.Doctors.Queries.GetDoctors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Api.Controllers;

[Authorize]
public class DoctorsController : ApiController
{
    /// <summary>
    /// Retrieves all doctors (cached via HybridCache).
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Doctor,Receptionist,Patient")]
    [ProducesResponseType(typeof(IReadOnlyList<DoctorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetDoctorsQuery(activeOnly), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a doctor by ID (cached via HybridCache).
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Doctor,Receptionist,Patient")]
    [ProducesResponseType(typeof(DoctorDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetDoctorByIdQuery(id), cancellationToken);
        if (result == null) return NotFound(new { message = $"Doctor with ID '{id}' not found." });
        return Ok(result);
    }

    /// <summary>
    /// Registers a new doctor into the clinic system and invalidates the doctors cache.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Doctor,Receptionist")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateDoctorCommand command, CancellationToken cancellationToken)
    {
        var doctorId = await Mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = doctorId }, new { id = doctorId });
    }
}
