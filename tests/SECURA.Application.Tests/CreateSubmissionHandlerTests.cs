using Moq;
using SECURA.Application.Common.Interfaces;
using SECURA.Application.Submissions.Commands;
using SECURA.Domain.Common;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Application.Tests;

public sealed class CreateSubmissionHandlerTests
{
    private readonly Mock<ISubmissionRepository> _submissionRepo = new();
    private readonly Mock<IAgencyRepository> _agencyRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    public CreateSubmissionHandlerTests()
    {
        _currentUser.Setup(u => u.UserId).Returns("uw-user");
        _submissionRepo
            .Setup(r => r.AddAsync(It.IsAny<Submission>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private CreateSubmissionCommandHandler CreateHandler() =>
        new(_submissionRepo.Object, _agencyRepo.Object, _unitOfWork.Object, _currentUser.Object);

    [Fact]
    public async Task Handle_Creates_Submission_With_New_Status()
    {
        var agencyId = Guid.NewGuid();
        _agencyRepo.Setup(r => r.ExistsAsync(agencyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        Submission? captured = null;
        _submissionRepo
            .Setup(r => r.AddAsync(It.IsAny<Submission>(), It.IsAny<CancellationToken>()))
            .Callback<Submission, CancellationToken>((s, _) => captured = s)
            .Returns(Task.CompletedTask);

        var command = new CreateSubmissionCommand(
            agencyId, LobType.BOP, "TX", DateTime.UtcNow,
            "Acme Corp", null);

        var id = await CreateHandler().Handle(command, CancellationToken.None);

        id.Should().NotBeEmpty();
        captured.Should().NotBeNull();
        captured!.Status.Should().Be(SubmissionStatus.New);
        captured.InsuredName.Should().Be("Acme Corp");
    }

    [Fact]
    public async Task Handle_Throws_When_Agency_Not_Found()
    {
        _agencyRepo.Setup(r => r.ExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var act = async () => await CreateHandler().Handle(
            new CreateSubmissionCommand(Guid.NewGuid(), LobType.BOP, "TX",
                DateTime.UtcNow, null, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
