using ClinicSystem.Application.Patients.Commands.RegisterPatient;
using ClinicSystem.Application.Patients.Queries.GetPatientById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Api.Controllers
{
    [Authorize]
    public class PatientsController : ApiController
    {
        /// <summary>
        /// Registers a new patient in the clinic system.
        /// </summary>
        /// <param name="command">Patient registration details.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The ID of the newly registered patient.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin,Doctor,Receptionist")]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterPatientCommand command, CancellationToken cancellationToken)
        {
            var patientId = await Mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = patientId }, new { id = patientId });
        }

        /// <summary>
        /// Retrieves a patient's details by their unique ID.
        /// </summary>
        /// <param name="id">The patient GUID.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Patient details.</returns>
        [HttpGet("{id:guid}")]
        [Authorize(Roles = "Admin,Doctor,Receptionist,Patient")]
        [ProducesResponseType(typeof(PatientDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await Mediator.Send(new GetPatientByIdQuery(id), cancellationToken);
            if (result == null)
            {
                return NotFound(new { message = $"Patient with ID '{id}' was not found." });
            }

            return Ok(result);
        }
    }
}
