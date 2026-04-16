using Moq;
using SECURA.Application.Common.Interfaces;
using SECURA.Application.Submissions.Commands;
using SECURA.Domain.Common;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Application.Tests;

public sealed class AddUWNoteHandlerTests
{
    private readonly Mock<ISubmissionRepository> _submissionRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    public AddUWNoteHandlerTests()
    {
        _currentUser.Setup(u => u.UserId).Returns("underwriter-1");
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private AddUWNoteCommandHandler CreateHandler() =>
        new(_submissionRepo.Object, _unitOfWork.Object, _currentUser.Object);

    [Fact]
    public async Task Handle_Adds_Note_And_Returns_NoteId()
    {
        var agencyId = Guid.NewGuid();
        var submission = Submission.Create(agencyId, LobType.BOP, "TX",
            DateTime.UtcNow, "Insured Inc");

        _submissionRepo.Setup(r => r.GetByIdAsync(submission.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(submission);

        var command = new AddUWNoteCommand(submission.Id, "Needs additional loss runs.");
        var noteId = await CreateHandler().Handle(command, CancellationToken.None);

        noteId.Should().NotBeEmpty();
        submission.UWNotes.Should().HaveCount(1);
        submission.UWNotes[0].NoteText.Should().Be("Needs additional loss runs.");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_When_Submission_Not_Found()
    {
        _submissionRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Submission?)null);

        var act = async () => await CreateHandler().Handle(
            new AddUWNoteCommand(Guid.NewGuid(), "Some note"),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
