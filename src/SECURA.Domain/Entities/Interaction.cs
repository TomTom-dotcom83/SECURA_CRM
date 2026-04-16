using SECURA.Domain.Common;
using SECURA.Domain.Enums;

namespace SECURA.Domain.Entities;

public sealed class Interaction : Entity<Guid>, IAuditableEntity
{
    private Interaction() { }

    private Interaction(Guid id, string relatedEntityType, Guid relatedEntityId,
        InteractionType interactionType, string summary, string createdByUserId) : base(id)
    {
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;
        InteractionType = interactionType;
        Summary = summary;
        Timestamp = DateTime.UtcNow;
        CreatedByUserId = createdByUserId;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdByUserId;
    }

    public string RelatedEntityType { get; private set; } = string.Empty;
    public Guid RelatedEntityId { get; private set; }
    public InteractionType InteractionType { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Summary { get; private set; } = string.Empty;
    public string? DetailNotes { get; private set; }
    public string CreatedByUserId { get; private set; } = string.Empty;
    public string? CreatedByDisplayName { get; private set; }
    public DateTime? DueDate { get; private set; }
    public bool IsCompleted { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    public static Interaction Create(string relatedEntityType, Guid relatedEntityId,
        InteractionType type, string summary, string userId)
    {
        if (string.IsNullOrWhiteSpace(summary))
            throw new DomainException("Interaction summary is required.");

        return new Interaction(Guid.NewGuid(), relatedEntityType, relatedEntityId, type, summary, userId);
    }

    public static Interaction CreateTask(string relatedEntityType, Guid relatedEntityId,
        string summary, DateTime dueDate, string userId)
    {
        var interaction = new Interaction(Guid.NewGuid(), relatedEntityType, relatedEntityId,
            InteractionType.Task, summary, userId)
        {
            DueDate = dueDate
        };
        return interaction;
    }

    public void Complete(string userId)
    {
        IsCompleted = true;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;
    }
}
