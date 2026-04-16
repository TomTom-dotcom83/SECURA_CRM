using MediatR;
using SECURA.Application.Submissions.Commands;
using SECURA.Application.Submissions.DTOs;
using SECURA.Application.Submissions.Queries;
using SECURA.Domain.Enums;

namespace SECURA.Infrastructure.Tests;

[Trait("Category", "E2E")]
[Collection("E2E")]
public sealed class SubmissionLifecycleTests : IClassFixture<SecuraWebApplicationFactory>
{
    private readonly SecuraWebApplicationFactory _factory;

    public SubmissionLifecycleTests(SecuraWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Submission_New_To_Bound_With_UWNote_Recorded()
    {
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Create agency first
        var agencyId = await mediator.Send(new Application.Agencies.Commands.CreateAgencyCommand(
            "Test Agency E2E", AgencyTier.Standard, "TX", null, null, null));

        // Create submission
        var submissionId = await mediator.Send(new CreateSubmissionCommand(
            agencyId, LobType.BOP, "TX", DateTime.UtcNow, "Insured E2E Corp", null));

        submissionId.Should().NotBeEmpty();

        // Add a UW note
        var noteId = await mediator.Send(new AddUWNoteCommand(submissionId, "Initial triage review."));
        noteId.Should().NotBeEmpty();

        // Walk through state machine: New → Triaged → InReview → Quoted → Bound
        await mediator.Send(new TransitionSubmissionStatusCommand(submissionId, SubmissionStatus.Triaged));
        await mediator.Send(new TransitionSubmissionStatusCommand(submissionId, SubmissionStatus.InReview));
        await mediator.Send(new TransitionSubmissionStatusCommand(submissionId, SubmissionStatus.Quoted));
        await mediator.Send(new TransitionSubmissionStatusCommand(submissionId, SubmissionStatus.Bound));

        // Verify final state
        var dto = await mediator.Send(new GetSubmissionByIdQuery(submissionId));

        dto.Should().NotBeNull();
        dto!.Status.Should().Be(SubmissionStatus.Bound);
        dto.UWNotes.Should().HaveCount(1);
        dto.UWNotes[0].NoteText.Should().Be("Initial triage review.");
    }
}
