using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Application.Common.Interfaces;

public interface IInteractionRepository : IRepository<Interaction, Guid>
{
    Task<IReadOnlyList<Interaction>> GetTimelineAsync(
        string relatedEntityType,
        Guid relatedEntityId,
        int maxResults = 50,
        CancellationToken cancellationToken = default);
}
