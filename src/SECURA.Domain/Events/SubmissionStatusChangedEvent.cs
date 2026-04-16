using SECURA.Domain.Common;
using SECURA.Domain.Enums;

namespace SECURA.Domain.Events;

public sealed record SubmissionStatusChangedEvent(
    Guid SubmissionId,
    SubmissionStatus? PreviousStatus,
    SubmissionStatus NewStatus) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
