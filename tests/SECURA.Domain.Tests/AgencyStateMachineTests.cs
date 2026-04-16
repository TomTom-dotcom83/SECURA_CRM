using FluentAssertions;
using SECURA.Domain.Common;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Domain.Tests;

public sealed class AgencyStateMachineTests
{
    [Fact]
    public void Agency_Can_Transition_Forward_Through_Full_Onboarding()
    {
        var agency = Agency.Create("Test Agency", AgencyTier.Standard, "TX");

        agency.Status.Should().Be(AgencyStatus.Intake);

        agency.Transition(AgencyStatus.Validation, "user1");
        agency.Status.Should().Be(AgencyStatus.Validation);

        agency.Transition(AgencyStatus.Contracting, "user1");
        agency.Status.Should().Be(AgencyStatus.Contracting);

        agency.Transition(AgencyStatus.Appointment, "user1");
        agency.Status.Should().Be(AgencyStatus.Appointment);

        agency.Transition(AgencyStatus.Training, "user1");
        agency.Status.Should().Be(AgencyStatus.Training);

        agency.Transition(AgencyStatus.Active, "user1");
        agency.Status.Should().Be(AgencyStatus.Active);
    }

    [Fact]
    public void Agency_Can_Be_Rejected_From_Any_Onboarding_State()
    {
        var onboardingStates = new[]
        {
            AgencyStatus.Intake, AgencyStatus.Validation,
            AgencyStatus.Contracting, AgencyStatus.Appointment, AgencyStatus.Training
        };

        foreach (var startState in onboardingStates)
        {
            var agency = Agency.Create($"Agency {startState}", AgencyTier.Standard, "CA");

            // Advance to the desired state
            AdvanceTo(agency, startState);

            var act = () => agency.Transition(AgencyStatus.Rejected, "admin");
            act.Should().NotThrow($"should be able to reject from {startState}");
            agency.Status.Should().Be(AgencyStatus.Rejected);
        }
    }

    [Fact]
    public void Agency_Cannot_Transition_Backward()
    {
        var agency = Agency.Create("Test Agency", AgencyTier.Standard, "NY");
        agency.Transition(AgencyStatus.Validation, "user1");

        var act = () => agency.Transition(AgencyStatus.Intake, "user1");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Active_Agency_Can_Be_Suspended()
    {
        var agency = Agency.Create("Test Agency", AgencyTier.Preferred, "FL");
        AdvanceTo(agency, AgencyStatus.Active);

        agency.Transition(AgencyStatus.Suspended, "admin");
        agency.Status.Should().Be(AgencyStatus.Suspended);
    }

    [Fact]
    public void Rejected_Agency_Cannot_Transition()
    {
        var agency = Agency.Create("Test Agency", AgencyTier.Standard, "TX");
        agency.Transition(AgencyStatus.Rejected, "user1");

        var act = () => agency.Transition(AgencyStatus.Intake, "user1");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Agency_Activation_Raises_DomainEvent()
    {
        var agency = Agency.Create("Test Agency", AgencyTier.Standard, "TX");
        AdvanceTo(agency, AgencyStatus.Training);

        agency.Transition(AgencyStatus.Active, "user1");

        agency.DomainEvents.Should().Contain(e => e is Events.AgencyActivatedEvent);
    }

    private static void AdvanceTo(Agency agency, AgencyStatus target)
    {
        var path = new[]
        {
            AgencyStatus.Intake, AgencyStatus.Validation, AgencyStatus.Contracting,
            AgencyStatus.Appointment, AgencyStatus.Training, AgencyStatus.Active
        };

        foreach (var state in path)
        {
            if (agency.Status == target) return;
            if (agency.CanTransitionTo(state))
                agency.Transition(state, "test");
        }
    }
}
