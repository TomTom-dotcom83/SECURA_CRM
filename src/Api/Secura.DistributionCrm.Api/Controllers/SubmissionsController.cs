using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Secura.DistributionCrm.Api.Auth;
using Secura.DistributionCrm.SharedKernel.Enums;
using Secura.DistributionCrm.Submissions.Application.Commands;
using Secura.DistributionCrm.Submissions.Application.Queries;

namespace Secura.DistributionCrm.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = SecuraPolicies.CanViewSubmissions)]
public sealed class SubmissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubmissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetSubmissions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] SubmissionStatus? status = null,
        [FromQuery] LobType? lob = null,
        [FromQuery] string? state = null,
        [FromQuery] Guid? agencyId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(
            new GetSubmissionsQuery(page, pageSize, status, lob, state, agencyId),
            cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSubmission(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSubmissionByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSubmission(
        [FromBody] CreateSubmissionCommand command, CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetSubmission), new { id }, new { id });
    }

    [HttpPost("{id:guid}/status")]
    [Authorize(Policy = SecuraPolicies.CanApproveSubmissions)]
    public async Task<IActionResult> TransitionStatus(
        Guid id, [FromBody] TransitionSubmissionRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new TransitionSubmissionStatusCommand(id, request.TargetStatus),
            cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/notes")]
    public async Task<IActionResult> AddNote(
        Guid id, [FromBody] AddNoteRequest request,
        CancellationToken cancellationToken)
    {
        var noteId = await _mediator.Send(
            new AddUWNoteCommand(id, request.NoteText), cancellationToken);
        return Created(string.Empty, new { id = noteId });
    }

    [HttpPost("{id:guid}/refer")]
    [Authorize(Policy = SecuraPolicies.CanApproveSubmissions)]
    public async Task<IActionResult> Refer(
        Guid id, [FromBody] ReferRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(new ReferSubmissionCommand(id, request.Reason), cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/request-document")]
    public async Task<IActionResult> RequestDocument(
        Guid id, [FromBody] RequestDocumentRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new RequestDocumentCommand(id, request.DocumentType, request.Notes ?? string.Empty),
            cancellationToken);
        return NoContent();
    }
}

public sealed record TransitionSubmissionRequest(SubmissionStatus TargetStatus);
public sealed record AddNoteRequest(string NoteText);
public sealed record ReferRequest(string Reason);
public sealed record RequestDocumentRequest(string DocumentType, string? Notes);
