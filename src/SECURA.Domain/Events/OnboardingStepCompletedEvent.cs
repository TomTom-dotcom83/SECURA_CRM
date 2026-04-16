using SECURA.Domain.Common;

namespace SECURA.Domain.Events;

public sealed record OnboardingStepCompletedEvent(
    Guid ChecklistId,
    Guid AgencyId,
    Guid ItemId,
    string CompletedByUserId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
