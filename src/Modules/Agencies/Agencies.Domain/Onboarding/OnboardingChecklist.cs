using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Primitives;
using Secura.DistributionCrm.Agencies.Domain.Agencies;
using Secura.DistributionCrm.Agencies.Domain.Events;

namespace Secura.DistributionCrm.Agencies.Domain.Onboarding;

public sealed class OnboardingChecklist : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<OnboardingChecklistItem> _items = [];

    private OnboardingChecklist() { }

    private OnboardingChecklist(Guid id, Guid agencyId, string templateName) : base(id)
    {
        AgencyId = agencyId;
        TemplateName = templateName;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = string.Empty;
    }

    public Guid AgencyId { get; private set; }
    public string TemplateName { get; private set; } = string.Empty;
    public bool IsComplete => _items.Count > 0 && _items.All(i => i.IsCompleted);

    public IReadOnlyList<OnboardingChecklistItem> Items => _items.AsReadOnly();

    public Agency? Agency { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    public static OnboardingChecklist Create(Guid agencyId, string templateName)
    {
        return new OnboardingChecklist(Guid.NewGuid(), agencyId, templateName);
    }

    public OnboardingChecklistItem AddItem(string stepName, string? description = null, int order = 0)
    {
        var item = OnboardingChecklistItem.Create(Id, stepName, description, order);
        _items.Add(item);
        return item;
    }

    public void CompleteItem(Guid itemId, string userId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId)
            ?? throw new DomainException($"Checklist item {itemId} not found.");

        item.Complete(userId);

        AddDomainEvent(new OnboardingStepCompletedEvent(Id, AgencyId, itemId, userId));

        if (IsComplete)
            AddDomainEvent(new AllChecklistStepsCompletedEvent(Id, AgencyId, userId));
    }
}
