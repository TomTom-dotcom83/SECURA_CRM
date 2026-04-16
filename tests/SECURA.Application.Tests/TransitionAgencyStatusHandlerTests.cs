using Moq;
using SECURA.Application.Agencies.Commands;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Common;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Application.Tests;

public sealed class TransitionAgencyStatusHandlerTests
{
    private readonly Mock<IAgencyRepository> _agencyRepo = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ICurrentUser> _currentUser = new();

    public TransitionAgencyStatusHandlerTests()
    {
        _currentUser.Setup(u => u.UserId).Returns("test-user");
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    private TransitionAgencyStatusCommandHandler CreateHandler() =>
        new(_agencyRepo.Object, _unitOfWork.Object, _currentUser.Object);

    [Fact]
    public async Task Handle_Transitions_Agency_Forward()
    {
        var agency = Agency.Create("Test Agency", AgencyTier.Standard, "TX");
        // Intake → Validation
        _agencyRepo.Setup(r => r.GetByIdAsync(agency.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agency);

        var command = new TransitionAgencyStatusCommand(agency.Id, AgencyStatus.Validation, null);
        await CreateHandler().Handle(command, CancellationToken.None);

        agency.Status.Should().Be(AgencyStatus.Validation);
        _agencyRepo.Verify(r => r.Update(agency), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Throws_When_Agency_Not_Found()
    {
        _agencyRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Agency?)null);

        var act = async () => await CreateHandler().Handle(
            new TransitionAgencyStatusCommand(Guid.NewGuid(), AgencyStatus.Active, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_Throws_DomainException_On_Invalid_Transition()
    {
        var agency = Agency.Create("Test Agency", AgencyTier.Standard, "TX");
        _agencyRepo.Setup(r => r.GetByIdAsync(agency.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agency);

        // Cannot go from Intake directly to Active (must follow state machine)
        var act = async () => await CreateHandler().Handle(
            new TransitionAgencyStatusCommand(agency.Id, AgencyStatus.Active, null),
            CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
    }
}
