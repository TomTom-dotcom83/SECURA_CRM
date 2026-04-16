using SECURA.Domain.Entities;

namespace SECURA.Application.Common.Interfaces;

public interface IDocumentRepository : IRepository<DocumentMetadata, Guid>
{
    Task<IReadOnlyList<DocumentMetadata>> GetByRelatedEntityAsync(
        string relatedEntityType,
        Guid relatedEntityId,
        CancellationToken cancellationToken = default);
}
