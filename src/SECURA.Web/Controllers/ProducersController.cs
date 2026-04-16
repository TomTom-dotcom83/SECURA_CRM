using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SECURA.Application.Producers.Commands;
using SECURA.Application.Producers.Queries;
using SECURA.Domain.Enums;
using SECURA.Web.Auth;

namespace SECURA.Web.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = SecuraPolicies.ReadOnly)]
public sealed class ProducersController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProducersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] Guid? agencyId = null,
        [FromQuery] LicenseStatus? licenseStatus = null,
        [FromQuery] bool? activeOnly = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetProducersQuery(page, pageSize, agencyId, licenseStatus, activeOnly, search),
            cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = SecuraPolicies.CanManageAgencies)]
    public async Task<IActionResult> CreateProducer(
        [FromBody] CreateProducerCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProducer), new { id }, new { id });
    }

    [HttpGet("{id:guid}", Name = nameof(GetProducer))]
    public IActionResult GetProducer(Guid id) =>
        Ok(new { message = "Not yet implemented", id });

    [HttpPost("{id:guid}/licenses")]
    [Authorize(Policy = SecuraPolicies.CanManageAgencies)]
    public async Task<IActionResult> AddLicense(
        Guid id, [FromBody] AddLicenseCommand command, CancellationToken cancellationToken)
    {
        var licenseId = await _mediator.Send(command with { ProducerId = id }, cancellationToken);
        return Created(string.Empty, new { id = licenseId });
    }
}
