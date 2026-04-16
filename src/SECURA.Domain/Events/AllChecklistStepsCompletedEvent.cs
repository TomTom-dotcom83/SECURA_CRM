using SECURA.Domain.Common;

namespace SECURA.Domain.Events;

public sealed record AllChecklistStepsCompletedEvent(
    Guid ChecklistId,
    Guid AgencyId,
    string CompletedByUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
