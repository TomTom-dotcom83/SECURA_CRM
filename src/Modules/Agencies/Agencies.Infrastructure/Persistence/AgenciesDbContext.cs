using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Secura.DistributionCrm.Agencies.Domain.Agencies;
using Secura.DistributionCrm.Agencies.Domain.Onboarding;
using Secura.DistributionCrm.Agencies.Domain.Producers;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence;

namespace Secura.DistributionCrm.Agencies.Infrastructure.Persistence;

public sealed class AgenciesDbContext : BaseDbContext
{
    public AgenciesDbContext(
        DbContextOptions<AgenciesDbContext> options,
        ICurrentUser? currentUser = null)
        : base(options, currentUser)
    {
    }

    public DbSet<Agency> Agencies => Set<Agency>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<Producer> Producers => Set<Producer>();
    public DbSet<License> Licenses => Set<License>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<OnboardingChecklist> OnboardingChecklists => Set<OnboardingChecklist>();
    public DbSet<OnboardingChecklistItem> OnboardingChecklistItems => Set<OnboardingChecklistItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("agencies");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgenciesDbContext).Assembly);
    }
}
