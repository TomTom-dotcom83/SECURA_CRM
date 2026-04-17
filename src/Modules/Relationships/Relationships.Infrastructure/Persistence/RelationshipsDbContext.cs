using Microsoft.EntityFrameworkCore;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence;
using Secura.DistributionCrm.Relationships.Domain.Interactions;

namespace Secura.DistributionCrm.Relationships.Infrastructure.Persistence;

public sealed class RelationshipsDbContext : BaseDbContext
{
    public RelationshipsDbContext(
        DbContextOptions<RelationshipsDbContext> options,
        ICurrentUser? currentUser = null)
        : base(options, currentUser)
    {
    }

    public DbSet<Interaction> Interactions => Set<Interaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("relationships");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RelationshipsDbContext).Assembly);
    }
}
