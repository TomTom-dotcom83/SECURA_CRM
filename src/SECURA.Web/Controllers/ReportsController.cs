using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SECURA.Application.Reports.Queries;
using SECURA.Web.Auth;

namespace SECURA.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = SecuraPolicies.CanViewReports)]
public sealed class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("agency-scorecard")]
    public async Task<IActionResult> GetAgencyScorecard(
        [FromQuery] Guid agencyId, CancellationToken cancellationToken)
    {
        var report = await _mediator.Send(new GetAgencyScorecardQuery(agencyId), cancellationToken);
        return Ok(report);
    }

    [HttpGet("pipeline")]
    public async Task<IActionResult> GetPipeline(CancellationToken cancellationToken)
    {
        var report = await _mediator.Send(new GetPipelineReportQuery(), cancellationToken);
        return Ok(report);
    }

    [HttpGet("compliance")]
    public async Task<IActionResult> GetCompliance(CancellationToken cancellationToken)
    {
        var report = await _mediator.Send(new GetComplianceSummaryQuery(), cancellationToken);
        return Ok(report);
    }

    [HttpGet("sla-adherence")]
    public async Task<IActionResult> GetSlaAdherence(CancellationToken cancellationToken)
    {
        var report = await _mediator.Send(new GetSlaAdherenceQuery(), cancellationToken);
        return Ok(report);
    }

    [HttpGet("onboarding-cycle-time")]
    public async Task<IActionResult> GetOnboardingCycleTime(CancellationToken cancellationToken)
    {
        var report = await _mediator.Send(new GetOnboardingCycleTimeQuery(), cancellationToken);
        return Ok(report);
    }
}
