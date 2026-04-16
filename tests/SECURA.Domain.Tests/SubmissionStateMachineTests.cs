using FluentAssertions;
using SECURA.Domain.Common;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Domain.Tests;

public sealed class SubmissionStateMachineTests
{
    [Fact]
    public void Submission_Lifecycle_New_To_Bound()
    {
        var submission = Submission.Create(Guid.NewGuid(), LobType.BOP, "TX", DateTime.UtcNow);

        submission.Transition(SubmissionStatus.Triaged, "user1");
        submission.Transition(SubmissionStatus.InReview, "user1");
        submission.Quote("Q-12345", 50000m, "uw1");
        submission.Transition(SubmissionStatus.Bound, "uw1");

        submission.Status.Should().Be(SubmissionStatus.Bound);
        submission.QuoteNumber.Should().Be("Q-12345");
        submission.QuotedPremium.Should().Be(50000m);
    }

    [Fact]
    public void Submission_Can_Be_Referred_And_Return_To_InReview()
    {
        var submission = Submission.Create(Guid.NewGuid(), LobType.CommercialAuto, "CA", DateTime.UtcNow);
        submission.Transition(SubmissionStatus.Triaged, "user1");
        submission.Transition(SubmissionStatus.InReview, "user1");
        submission.Refer("uw-manager-1", "uw1");

        submission.Status.Should().Be(SubmissionStatus.Referred);
        submission.ReferredToUserId.Should().Be("uw-manager-1");

        submission.Transition(SubmissionStatus.InReview, "uw-manager-1");
        submission.Status.Should().Be(SubmissionStatus.InReview);
    }

    [Fact]
    public void Submission_Cannot_Skip_From_New_To_Quoted()
    {
        var submission = Submission.Create(Guid.NewGuid(), LobType.BOP, "TX", DateTime.UtcNow);

        var act = () => submission.Transition(SubmissionStatus.Quoted, "user1");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Submission_IsOverdue_When_Past_SlaDeadline()
    {
        // Received 5 days ago with 48-hour SLA
        var submission = Submission.Create(Guid.NewGuid(), LobType.BOP, "TX",
            DateTime.UtcNow.AddDays(-5));
        submission.Transition(SubmissionStatus.Triaged, "user1");

        submission.IsOverdue.Should().BeTrue();
    }

    [Fact]
    public void Bound_Submission_Not_Overdue()
    {
        var submission = Submission.Create(Guid.NewGuid(), LobType.BOP, "TX",
            DateTime.UtcNow.AddDays(-5));
        submission.Transition(SubmissionStatus.Triaged, "user1");
        submission.Transition(SubmissionStatus.InReview, "user1");
        submission.Quote("Q-001", 1000m, "uw1");
        submission.Transition(SubmissionStatus.Bound, "uw1");

        submission.IsOverdue.Should().BeFalse();
    }

    [Fact]
    public void AddNote_Appended_To_UWNotes()
    {
        var submission = Submission.Create(Guid.NewGuid(), LobType.GeneralLiability, "NY",
            DateTime.UtcNow);

        submission.AddNote("First review — looks good.", "uw1");
        submission.AddNote("Referred for second opinion.", "uw2");

        submission.UWNotes.Should().HaveCount(2);
    }
}
