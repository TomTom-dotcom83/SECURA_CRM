using Microsoft.EntityFrameworkCore;
using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence;
using Secura.DistributionCrm.Documents.Domain;

namespace Secura.DistributionCrm.Documents.Infrastructure.Persistence;

public sealed class DocumentsDbContext : BaseDbContext
{
    public DocumentsDbContext(
        DbContextOptions<DocumentsDbContext> options,
        ICurrentUser? currentUser = null)
        : base(options, currentUser)
    {
    }

    public DbSet<DocumentMetadata> DocumentMetadata => Set<DocumentMetadata>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("documents");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocumentsDbContext).Assembly);
    }
}
