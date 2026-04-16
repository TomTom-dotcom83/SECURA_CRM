using MediatR;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;
using SECURA.Domain.Events;

namespace SECURA.Application.Compliance.EventHandlers;

public sealed class LicenseExpiredEventHandler
    : INotificationHandler<LicenseExpiredEvent>
{
    private readonly IRepository<Interaction, Guid> _interactions;
    private readonly IUnitOfWork _uow;

    public LicenseExpiredEventHandler(
        IRepository<Interaction, Guid> interactions,
        IUnitOfWork uow)
    {
        _interactions = interactions;
        _uow = uow;
    }

    public async Task Handle(LicenseExpiredEvent notification,
        CancellationToken cancellationToken)
    {
        var summary =
            $"License expired — State: {notification.State}, LOB: {notification.Lob}, " +
            $"Expiry: {notification.ExpirationDate:yyyy-MM-dd}. " +
            $"License ID: {notification.LicenseId}.";

        var interaction = Interaction.Create(
            "Producer",
            notification.ProducerId,
            InteractionType.Note,
            summary,
            "system");

        await _interactions.AddAsync(interaction, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
    }
}
