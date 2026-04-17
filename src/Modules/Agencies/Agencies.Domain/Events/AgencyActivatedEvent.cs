using Secura.DistributionCrm.BuildingBlocks.Domain.Events;

namespace Secura.DistributionCrm.Agencies.Domain.Events;

public sealed record AgencyActivatedEvent(
    Guid AgencyId,
    string ActivatedByUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
