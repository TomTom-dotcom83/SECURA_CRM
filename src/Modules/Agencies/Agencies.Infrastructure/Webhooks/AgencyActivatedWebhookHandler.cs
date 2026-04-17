using MediatR;
using Secura.DistributionCrm.Agencies.Domain.Events;

namespace Secura.DistributionCrm.Agencies.Infrastructure.Webhooks;

public sealed class AgencyActivatedWebhookHandler
    : INotificationHandler<AgencyActivatedEvent>
{
    private readonly WebhookDispatcher _dispatcher;

    public AgencyActivatedWebhookHandler(WebhookDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Handle(AgencyActivatedEvent notification,
        CancellationToken cancellationToken)
    {
        await _dispatcher.SendAsync("agency.activated", new
        {
            notification.AgencyId,
            notification.ActivatedByUserId,
            notification.OccurredOn
        }, cancellationToken);
    }
}
