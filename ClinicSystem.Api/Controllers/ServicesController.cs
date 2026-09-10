using ClinicSystem.Application.Services.Commands.CreateMedicalService;
using ClinicSystem.Application.Services.Queries;
using ClinicSystem.Application.Services.Queries.GetMedicalServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Api.Controllers;

[Authorize]
public class ServicesController : ApiController
{
    /// <summary>
    /// Retrieves all clinic medical services and pricing (cached via HybridCache).
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<MedicalServiceDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        var result = await Mediator.Send(new GetMedicalServicesQuery(activeOnly), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Creates a new medical service / procedure and invalidates service cache.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateMedicalServiceCommand command, CancellationToken cancellationToken)
    {
        var serviceId = await Mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new { serviceId });
    }
}
