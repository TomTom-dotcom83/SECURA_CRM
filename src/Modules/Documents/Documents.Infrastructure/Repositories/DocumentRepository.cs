using Microsoft.EntityFrameworkCore;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence;
using Secura.DistributionCrm.Documents.Application.Abstractions;
using Secura.DistributionCrm.Documents.Domain;
using Secura.DistributionCrm.Documents.Infrastructure.Persistence;

namespace Secura.DistributionCrm.Documents.Infrastructure.Repositories;

public sealed class DocumentRepository
    : Repository<DocumentMetadata, Guid, DocumentsDbContext>, IDocumentRepository
{
    public DocumentRepository(DocumentsDbContext context) : base(context) { }

    public async Task<IReadOnlyList<DocumentMetadata>> GetByRelatedEntityAsync(
        string relatedEntityType,
        Guid relatedEntityId,
        CancellationToken cancellationToken = default)
        => await DbSet
            .Where(d => d.RelatedEntityType == relatedEntityType &&
                        d.RelatedEntityId == relatedEntityId &&
                        d.IsActive)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync(cancellationToken);
}
