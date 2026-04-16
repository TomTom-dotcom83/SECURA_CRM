using MediatR;
using SECURA.Domain.Events;

namespace SECURA.Infrastructure.Webhooks;

public sealed class LicenseExpiredWebhookHandler
    : INotificationHandler<LicenseExpiredEvent>
{
    private readonly WebhookDispatcher _dispatcher;

    public LicenseExpiredWebhookHandler(WebhookDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task Handle(LicenseExpiredEvent notification,
        CancellationToken cancellationToken)
    {
        await _dispatcher.SendAsync("license.expired", new
        {
            notification.LicenseId,
            notification.ProducerId,
            notification.State,
            Lob = notification.Lob.ToString(),
            notification.ExpirationDate,
            notification.OccurredOn
        }, cancellationToken);
    }
}
