using Microsoft.EntityFrameworkCore;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Entities;
using SECURA.Infrastructure.Persistence;

namespace SECURA.Infrastructure.Repositories;

public sealed class InteractionRepository : Repository<Interaction, Guid>, IInteractionRepository
{
    public InteractionRepository(SecuraDbContext context) : base(context) { }

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
