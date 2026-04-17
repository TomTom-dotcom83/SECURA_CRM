using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Relationships.Application.Interactions;

public sealed class InteractionDto
{
    public Guid Id { get; init; }
    public string RelatedEntityType { get; init; } = string.Empty;
    public Guid RelatedEntityId { get; init; }
    public InteractionType InteractionType { get; init; }
    public DateTime Timestamp { get; init; }
    public string Summary { get; init; } = string.Empty;
    public string? DetailNotes { get; init; }
    public string CreatedByUserId { get; init; } = string.Empty;
    public string? CreatedByDisplayName { get; init; }
    public DateTime? DueDate { get; init; }
    public bool IsCompleted { get; init; }
}
