using Secura.DistributionCrm.BuildingBlocks.Domain.Events;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Domain.Events;

public sealed record LicenseExpiredEvent(
    Guid LicenseId,
    Guid ProducerId,
    string State,
    LobType Lob,
    DateTime ExpirationDate) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
