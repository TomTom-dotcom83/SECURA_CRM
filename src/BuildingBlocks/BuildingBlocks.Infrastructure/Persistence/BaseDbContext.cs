using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Primitives;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence.Outbox;

namespace Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence;

public abstract class BaseDbContext : DbContext, IUnitOfWork
{
    private readonly ICurrentUser? _currentUser;

    protected BaseDbContext(DbContextOptions options, ICurrentUser? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OutboxMessage>(b =>
        {
            b.ToTable("OutboxMessages");
            b.HasKey(o => o.Id);
            b.Property(o => o.Id).ValueGeneratedNever();
            b.Property(o => o.Type).IsRequired().HasMaxLength(500);
            b.Property(o => o.Payload).IsRequired().HasColumnType("nvarchar(max)");
            b.Property(o => o.Error).HasMaxLength(2000);
            b.HasIndex(o => o.ProcessedAt);
            b.HasIndex(o => o.OccurredOn);
            b.Ignore(o => o.IsProcessed);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var userId = _currentUser?.UserId ?? "system";
        var now = DateTime.UtcNow;

        // Audit timestamps
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
                entry.Entity.CreatedBy = userId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedAt = now;
                entry.Entity.ModifiedBy = userId;
            }
        }

        // Collect domain events from all aggregates and serialize to outbox
        var aggregates = ChangeTracker.Entries<AggregateRoot<Guid>>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count != 0)
            .ToList();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var outbox = OutboxMessage.Create(
                    domainEvent.GetType().FullName ?? domainEvent.GetType().Name,
                    JsonSerializer.Serialize(domainEvent, domainEvent.GetType()));
                OutboxMessages.Add(outbox);
            }
            aggregate.ClearDomainEvents();
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
