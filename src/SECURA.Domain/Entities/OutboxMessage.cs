using SECURA.Domain.Common;

namespace SECURA.Domain.Entities;

public sealed class OutboxMessage : Entity<Guid>
{
    private OutboxMessage() { }

    private OutboxMessage(Guid id, string type, string payload) : base(id)
    {
        Type = type;
        Payload = payload;
        OccurredOn = DateTime.UtcNow;
    }

    public string Type { get; private set; } = string.Empty;
    public string Payload { get; private set; } = string.Empty;
    public DateTime OccurredOn { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public string? Error { get; private set; }
    public int RetryCount { get; private set; }

    public bool IsProcessed => ProcessedAt.HasValue;

    public static OutboxMessage Create(string type, string payload) =>
        new(Guid.NewGuid(), type, payload);

    public void MarkProcessed()
    {
        ProcessedAt = DateTime.UtcNow;
        Error = null;
    }

    public void MarkFailed(string error)
    {
        Error = error;
        RetryCount++;
    }
}
