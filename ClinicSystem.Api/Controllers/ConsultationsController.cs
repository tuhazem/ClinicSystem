using ClinicSystem.Application.MedicalRecords.Commands.CreateConsultationRecord;
using ClinicSystem.Application.MedicalRecords.Queries;
using ClinicSystem.Application.MedicalRecords.Queries.GetPatientConsultations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Api.Controllers;

public class ConsultationsController : ApiController
{
    /// <summary>
    /// Retrieves all electronic medical consultation records and prescriptions for a patient (cached via HybridCache).
    /// </summary>
    [HttpGet("patient/{patientId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ConsultationRecordDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPatientConsultations(Guid patientId, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetPatientConsultationsQuery(patientId), cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Records a doctor consultation diagnosis, symptoms, notes, and medication prescriptions.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateConsultationRecordCommand command, CancellationToken cancellationToken)
    {
        var consultationId = await Mediator.Send(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, new { consultationId });
    }
}
