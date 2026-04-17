using Microsoft.EntityFrameworkCore;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence;
using Secura.DistributionCrm.Submissions.Domain;

namespace Secura.DistributionCrm.Administration.Infrastructure.Persistence;

public sealed class AdministrationDbContext : BaseDbContext
{
    public AdministrationDbContext(
        DbContextOptions<AdministrationDbContext> options,
        ICurrentUser? currentUser = null)
        : base(options, currentUser)
    {
    }

    public DbSet<AppetiteTag> AppetiteTags => Set<AppetiteTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("administration");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdministrationDbContext).Assembly);
    }
}
