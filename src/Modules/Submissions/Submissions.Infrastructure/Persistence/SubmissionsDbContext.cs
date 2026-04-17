using Microsoft.EntityFrameworkCore;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence;
using Secura.DistributionCrm.Submissions.Domain;

namespace Secura.DistributionCrm.Submissions.Infrastructure.Persistence;

public sealed class SubmissionsDbContext : BaseDbContext
{
    public SubmissionsDbContext(
        DbContextOptions<SubmissionsDbContext> options,
        ICurrentUser? currentUser = null)
        : base(options, currentUser)
    {
    }

    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<UWNote> UWNotes => Set<UWNote>();
    public DbSet<AppetiteTag> AppetiteTags => Set<AppetiteTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("submissions");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SubmissionsDbContext).Assembly);
    }
}
