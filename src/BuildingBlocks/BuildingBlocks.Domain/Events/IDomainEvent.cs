using MediatR;

namespace Secura.DistributionCrm.BuildingBlocks.Domain.Events;

/// <summary>
/// Marker interface for domain events. Extends MediatR INotification so
/// handlers can subscribe directly via INotificationHandler&lt;T&gt;.
/// </summary>
public interface IDomainEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
