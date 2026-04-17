using Secura.DistributionCrm.BuildingBlocks.Domain.Events;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Submissions.Domain.Events;

public sealed record SubmissionStatusChangedEvent(
    Guid SubmissionId,
    SubmissionStatus? PreviousStatus,
    SubmissionStatus NewStatus) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
