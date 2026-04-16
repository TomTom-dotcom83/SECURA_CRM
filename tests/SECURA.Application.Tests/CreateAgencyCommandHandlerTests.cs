using FluentAssertions;
using Moq;
using SECURA.Application.Agencies.Commands;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Application.Tests;

public sealed class CreateAgencyCommandHandlerTests
{
    private readonly Mock<IAgencyRepository> _agencyRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    private CreateAgencyCommandHandler CreateHandler() =>
        new(_agencyRepo.Object, _unitOfWork.Object, _currentUser.Object);

    public CreateAgencyCommandHandlerTests()
    {
        _currentUser.Setup(u => u.UserId).Returns("test-user");
        _agencyRepo.Setup(r => r.AddAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    [Fact]
    public async Task Handle_Creates_Agency_And_Returns_Id()
    {
        var command = new CreateAgencyCommand("Acme Insurance", AgencyTier.Standard, "TX",
            null, null, null);
        var handler = CreateHandler();

        var id = await handler.Handle(command, CancellationToken.None);

        id.Should().NotBeEmpty();
        _agencyRepo.Verify(r => r.AddAsync(
            It.Is<Agency>(a => a.Name == "Acme Insurance" && a.PrimaryState == "TX"),
            It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Initial_Status_Is_Intake()
    {
        Agency? captured = null;
        _agencyRepo.Setup(r => r.AddAsync(It.IsAny<Agency>(), It.IsAny<CancellationToken>()))
            .Callback<Agency, CancellationToken>((a, _) => captured = a)
            .Returns(Task.CompletedTask);

        var command = new CreateAgencyCommand("Test Agency", AgencyTier.Preferred, "NY",
            null, null, null);
        await CreateHandler().Handle(command, CancellationToken.None);

        captured.Should().NotBeNull();
        captured!.Status.Should().Be(AgencyStatus.Intake);
    }
}
