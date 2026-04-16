using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SECURA.Application.Producers.Commands;
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
