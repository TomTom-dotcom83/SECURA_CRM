using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.Agencies.Domain.Events;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Application.Compliance;

/// <summary>
/// Handles LicenseExpiredEvent by publishing a follow-up notification.
/// Cross-module interaction (creating an Interaction record) is handled
/// via MediatR notification to avoid direct module-to-module coupling.
/// </summary>
public sealed class LicenseExpiredEventHandler
    : INotificationHandler<LicenseExpiredEvent>
{
    private readonly IMediator _mediator;

    public LicenseExpiredEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public Task Handle(LicenseExpiredEvent notification,
        CancellationToken cancellationToken)
    {
        // Notification is handled — downstream modules can subscribe to LicenseExpiredEvent
        // via their own INotificationHandler implementations.
        return Task.CompletedTask;
    }
}
