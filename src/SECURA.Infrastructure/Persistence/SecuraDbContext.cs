using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Common;
using SECURA.Domain.Entities;

namespace SECURA.Infrastructure.Persistence;

public sealed class SecuraDbContext : DbContext, IUnitOfWork
{
    private readonly ICurrentUser? _currentUser;

    public SecuraDbContext(DbContextOptions<SecuraDbContext> options, ICurrentUser? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Producer> Producers => Set<Producer>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<UWNote> UWNotes => Set<UWNote>();
    public DbSet<Interaction> Interactions => Set<Interaction>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<OnboardingChecklist> OnboardingChecklists => Set<OnboardingChecklist>();
    public DbSet<OnboardingChecklistItem> OnboardingChecklistItems => Set<OnboardingChecklistItem>();
    public DbSet<AppetiteTag> AppetiteTags => Set<AppetiteTag>();
    public DbSet<ClaimReference> ClaimReferences => Set<ClaimReference>();
    public DbSet<DocumentMetadata> DocumentMetadata => Set<DocumentMetadata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SecuraDbContext).Assembly);
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
