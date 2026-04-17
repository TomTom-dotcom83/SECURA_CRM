using Microsoft.EntityFrameworkCore;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence;
using Secura.DistributionCrm.Relationships.Application.Abstractions;
using Secura.DistributionCrm.Relationships.Domain.Interactions;
using Secura.DistributionCrm.Relationships.Infrastructure.Persistence;

namespace Secura.DistributionCrm.Relationships.Infrastructure.Repositories;

public sealed class InteractionRepository
    : Repository<Interaction, Guid, RelationshipsDbContext>, IInteractionRepository
{
    public InteractionRepository(RelationshipsDbContext context) : base(context) { }

    public async Task<IReadOnlyList<Interaction>> GetTimelineAsync(
        string relatedEntityType,
        Guid relatedEntityId,
        int maxResults = 50,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Where(i => i.RelatedEntityType == relatedEntityType &&
                        i.RelatedEntityId == relatedEntityId)
            .OrderByDescending(i => i.Timestamp)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
}
