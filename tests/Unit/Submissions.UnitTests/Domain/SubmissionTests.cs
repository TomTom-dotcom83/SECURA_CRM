using FluentAssertions;
using Secura.DistributionCrm.SharedKernel.Enums;
using Secura.DistributionCrm.Submissions.Domain;
using Xunit;

namespace Secura.DistributionCrm.Submissions.UnitTests.Domain;

public sealed class SubmissionTests
{
    [Fact]
    public void Create_WithValidArgs_ReturnsSubmissionInNewStatus()
    {
        var submission = Submission.Create(
            Guid.NewGuid(), LobType.BOP, "CA", DateTime.UtcNow);

        submission.Status.Should().Be(SubmissionStatus.New);
        submission.Lob.Should().Be(LobType.BOP);
    }

    [Fact]
    public void Transition_NewToTriaged_Succeeds()
    {
        var submission = Submission.Create(
            Guid.NewGuid(), LobType.BOP, "CA", DateTime.UtcNow);

        submission.Transition(SubmissionStatus.Triaged, "user1");

        submission.Status.Should().Be(SubmissionStatus.Triaged);
    }

    [Fact]
    public void Transition_InvalidPath_ThrowsDomainException()
    {
        var submission = Submission.Create(
            Guid.NewGuid(), LobType.BOP, "CA", DateTime.UtcNow);

        var act = () => submission.Transition(SubmissionStatus.Bound, "user1");

        act.Should().Throw<Exception>().WithMessage("*Cannot transition*");
    }

    [Fact]
    public void SlaDeadline_BOP_Is48HoursAfterReceived()
    {
        var received = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var submission = Submission.Create(Guid.NewGuid(), LobType.BOP, "CA", received);

        submission.SlaDeadline.Should().Be(received.AddHours(48));
    }

    [Fact]
    public void AddNote_ReturnsNoteWithCorrectText()
    {
        var submission = Submission.Create(
            Guid.NewGuid(), LobType.BOP, "CA", DateTime.UtcNow);

        var note = submission.AddNote("Test note text", "author1");

        note.NoteText.Should().Be("Test note text");
        note.AuthorUserId.Should().Be("author1");
        submission.UWNotes.Should().HaveCount(1);
    }
}
