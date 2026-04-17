using Microsoft.EntityFrameworkCore;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence;
using Secura.DistributionCrm.Claims.Domain;

namespace Secura.DistributionCrm.Claims.Infrastructure.Persistence;

public sealed class ClaimsDbContext : BaseDbContext
{
    public ClaimsDbContext(
        DbContextOptions<ClaimsDbContext> options,
        ICurrentUser? currentUser = null)
        : base(options, currentUser)
    {
    }

    public DbSet<ClaimReference> ClaimReferences => Set<ClaimReference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("claims");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClaimsDbContext).Assembly);
    }
}
