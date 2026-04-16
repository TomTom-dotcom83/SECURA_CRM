using MediatR;
using SECURA.Application.Agencies.Commands;
using SECURA.Application.Compliance.Commands;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;
using SECURA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SECURA.Infrastructure.Tests;

[Trait("Category", "E2E")]
[Collection("E2E")]
public sealed class OnboardingWorkflowTests : IClassFixture<SecuraWebApplicationFactory>
{
    private readonly SecuraWebApplicationFactory _factory;

    public OnboardingWorkflowTests(SecuraWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Agency_Can_Be_Activated_After_Completing_Checklist()
    {
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<SecuraDbContext>();

        // Create agency
        var agencyId = await mediator.Send(new CreateAgencyCommand(
            "Onboarding E2E Agency", AgencyTier.Standard, "CA", null, null, null));

        // Create and complete a checklist manually via domain
        var checklist = OnboardingChecklist.Create(agencyId, "Standard");
        var item = checklist.AddItem("Verify E&O Certificate", null, 1);
        db.OnboardingChecklists.Add(checklist);
        await db.SaveChangesAsync();

        // Complete the checklist item
        await mediator.Send(new CompleteChecklistItemCommand(checklist.Id, item.Id));

        // Advance agency through state machine to Training
        await mediator.Send(new TransitionAgencyStatusCommand(agencyId, AgencyStatus.Validation, null));
        await mediator.Send(new TransitionAgencyStatusCommand(agencyId, AgencyStatus.Contracting, null));
        await mediator.Send(new TransitionAgencyStatusCommand(agencyId, AgencyStatus.Appointment, null));
        await mediator.Send(new TransitionAgencyStatusCommand(agencyId, AgencyStatus.Training, null));

        // Trigger activation
        await mediator.Send(new TriggerAgencyActivationCommand(agencyId));

        // Verify agency is Active
        var agency = await db.Agencies.FindAsync(agencyId);
        agency.Should().NotBeNull();
        agency!.Status.Should().Be(AgencyStatus.Active);
    }
}
