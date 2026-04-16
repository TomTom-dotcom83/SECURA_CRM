using SECURA.Domain.Common;

namespace SECURA.Domain.Entities;

public sealed class AuditLog : Entity<Guid>
{
    private AuditLog() { }

    private AuditLog(Guid id, string entityType, string entityId, string action,
        string userId, string? beforeJson, string? afterJson) : base(id)
    {
        EntityType = entityType;
        EntityId = entityId;
        Action = action;
        UserId = userId;
        Timestamp = DateTime.UtcNow;
        BeforeJson = beforeJson;
        AfterJson = afterJson;
    }

    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string UserId { get; private set; } = string.Empty;
    public string? UserDisplayName { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? BeforeJson { get; private set; }
    public string? AfterJson { get; private set; }
    public string? IpAddress { get; private set; }
    public string? CorrelationId { get; private set; }

    public static AuditLog Create(string entityType, string entityId, string action,
        string userId, string? beforeJson = null, string? afterJson = null)
    {
        return new AuditLog(Guid.NewGuid(), entityType, entityId, action, userId, beforeJson, afterJson);
    }
}
