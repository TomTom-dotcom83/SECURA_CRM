using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Primitives;

namespace Secura.DistributionCrm.Agencies.Domain.Onboarding;

public sealed class OnboardingChecklistItem : Entity<Guid>
{
    private OnboardingChecklistItem() { }

    private OnboardingChecklistItem(Guid id, Guid checklistId, string stepName,
        string? description, int order) : base(id)
    {
        ChecklistId = checklistId;
        StepName = stepName;
        Description = description;
        Order = order;
    }

    public Guid ChecklistId { get; private set; }
    public string StepName { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Order { get; private set; }
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? CompletedByUserId { get; private set; }
    public bool IsRequired { get; private set; } = true;

    public OnboardingChecklist? Checklist { get; private set; }

    public static OnboardingChecklistItem Create(Guid checklistId, string stepName,
        string? description = null, int order = 0)
    {
        if (string.IsNullOrWhiteSpace(stepName))
            throw new DomainException("Step name is required.");

        return new OnboardingChecklistItem(Guid.NewGuid(), checklistId, stepName.Trim(), description, order);
    }

    public void Complete(string userId)
    {
        if (IsCompleted)
            throw new DomainException($"Step '{StepName}' is already completed.");

        IsCompleted = true;
        CompletedAt = DateTime.UtcNow;
        CompletedByUserId = userId;
    }
}
