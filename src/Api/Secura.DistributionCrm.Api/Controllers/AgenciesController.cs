using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Secura.DistributionCrm.Agencies.Application.Commands;
using Secura.DistributionCrm.Agencies.Application.Queries;
using Secura.DistributionCrm.Api.Auth;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = SecuraPolicies.ReadOnly)]
public sealed class AgenciesController : ControllerBase
{
    private readonly IMediator _mediator;

    public AgenciesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAgencies(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] AgencyStatus? status = null,
        [FromQuery] AgencyTier? tier = null,
        [FromQuery] string? state = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetAgenciesQuery(page, pageSize, status, tier, state, search),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAgency(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAgencyByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = SecuraPolicies.CanManageAgencies)]
    public async Task<IActionResult> CreateAgency(
        [FromBody] CreateAgencyCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetAgency), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = SecuraPolicies.CanManageAgencies)]
    public async Task<IActionResult> UpdateAgency(
        Guid id, [FromBody] UpdateAgencyCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id) return BadRequest("Route ID does not match body ID.");
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = SecuraPolicies.CanManageAgencies)]
    public async Task<IActionResult> TransitionStatus(
        Guid id, [FromBody] TransitionAgencyStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new TransitionAgencyStatusCommand(id, request.TargetStatus, request.Reason),
            cancellationToken);
        return NoContent();
    }
}

public sealed record TransitionAgencyStatusRequest(AgencyStatus TargetStatus, string? Reason);
