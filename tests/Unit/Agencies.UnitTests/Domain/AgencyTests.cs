using FluentAssertions;
using Secura.DistributionCrm.Agencies.Domain.Agencies;
using Secura.DistributionCrm.SharedKernel.Enums;
using Xunit;

namespace Secura.DistributionCrm.Agencies.UnitTests.Domain;

public sealed class AgencyTests
{
    [Fact]
    public void Create_WithValidArgs_ReturnsAgencyInIntakeStatus()
    {
        var agency = Agency.Create("Test Agency", AgencyTier.Emerging, "CA");

        agency.Name.Should().Be("Test Agency");
        agency.Status.Should().Be(AgencyStatus.Intake);
        agency.Tier.Should().Be(AgencyTier.Emerging);
        agency.PrimaryState.Should().Be("CA");
    }

    [Fact]
    public void Create_WithEmptyName_ThrowsDomainException()
    {
        var act = () => Agency.Create("", AgencyTier.Emerging, "CA");

        act.Should().Throw<Exception>().WithMessage("*Agency name is required*");
    }

    [Fact]
    public void Transition_ValidTransition_UpdatesStatus()
    {
        var agency = Agency.Create("Test Agency", AgencyTier.Emerging, "CA");

        agency.Transition(AgencyStatus.Validation, "user1");

        agency.Status.Should().Be(AgencyStatus.Validation);
    }

    [Fact]
    public void Transition_InvalidTransition_ThrowsDomainException()
    {
        var agency = Agency.Create("Test Agency", AgencyTier.Emerging, "CA");

        var act = () => agency.Transition(AgencyStatus.Active, "user1");

        act.Should().Throw<Exception>().WithMessage("*Cannot transition*");
    }

    [Fact]
    public void Transition_ToActive_RaisesBothStatusChangedAndActivatedEvents()
    {
        var agency = Agency.Create("Test Agency", AgencyTier.Emerging, "CA");
        agency.Transition(AgencyStatus.Validation, "user1");
        agency.Transition(AgencyStatus.Contracting, "user1");
        agency.Transition(AgencyStatus.Appointment, "user1");
        agency.Transition(AgencyStatus.Training, "user1");
        agency.Transition(AgencyStatus.Active, "user1");

        agency.Status.Should().Be(AgencyStatus.Active);
        agency.DomainEvents.Should().Contain(e => e.GetType().Name == "AgencyActivatedEvent");
    }
}
