using SECURA.Domain.Common;
using SECURA.Domain.Enums;

namespace SECURA.Domain.Events;

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
