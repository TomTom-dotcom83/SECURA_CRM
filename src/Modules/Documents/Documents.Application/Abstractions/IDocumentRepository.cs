using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.Documents.Domain;

namespace Secura.DistributionCrm.Documents.Application.Abstractions;

public interface IDocumentRepository : IRepository<DocumentMetadata, Guid>
{
    Task<IReadOnlyList<DocumentMetadata>> GetByRelatedEntityAsync(
        string relatedEntityType,
        Guid relatedEntityId,
        CancellationToken cancellationToken = default);
}
