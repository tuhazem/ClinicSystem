using ClinicSystem.Application.Analytics.Queries.GetClinicDashboardSummary;
using ClinicSystem.Application.Common.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ClinicSystem.Api.Controllers;

[Authorize(Roles = "Admin,Doctor,Receptionist,Cashier")]
public class DashboardController : ApiController
{
    /// <summary>
    /// Retrieves real-time clinic operational and financial summary metrics (cached via HybridCache).
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(DashboardSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary([FromQuery] DateTime? date, CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetClinicDashboardSummaryQuery(date), cancellationToken);
        return Ok(result);
    }
}
