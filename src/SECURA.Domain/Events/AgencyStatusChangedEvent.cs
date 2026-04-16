using SECURA.Domain.Common;
using SECURA.Domain.Enums;

namespace SECURA.Domain.Events;

public sealed record AgencyStatusChangedEvent(
    Guid AgencyId,
    AgencyStatus? PreviousStatus,
    AgencyStatus NewStatus) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
