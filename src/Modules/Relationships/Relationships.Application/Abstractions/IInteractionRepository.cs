using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.Relationships.Domain.Interactions;

namespace Secura.DistributionCrm.Relationships.Application.Abstractions;

public interface IInteractionRepository : IRepository<Interaction, Guid>
{
    Task<IReadOnlyList<Interaction>> GetTimelineAsync(
        string relatedEntityType,
        Guid relatedEntityId,
        int maxResults = 50,
        CancellationToken cancellationToken = default);
}
