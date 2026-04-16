namespace SECURA.Domain.Common;

/// <summary>
/// Marker base class for aggregate roots. Outbox dispatcher uses this
/// to identify aggregates whose domain events should be serialized to
/// the OutboxMessages table during SaveChanges.
/// </summary>
public abstract class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    protected AggregateRoot(TId id) : base(id) { }
    protected AggregateRoot() { }
}
