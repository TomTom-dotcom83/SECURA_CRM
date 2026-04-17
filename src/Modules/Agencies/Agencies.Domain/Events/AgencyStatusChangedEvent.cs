using Secura.DistributionCrm.BuildingBlocks.Domain.Events;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Domain.Events;

public sealed record AgencyStatusChangedEvent(
    Guid AgencyId,
    AgencyStatus? PreviousStatus,
    AgencyStatus NewStatus) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
