using Microsoft.EntityFrameworkCore;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Entities;
using SECURA.Infrastructure.Persistence;

namespace SECURA.Infrastructure.Repositories;

public sealed class DocumentRepository : Repository<DocumentMetadata, Guid>, IDocumentRepository
{
    public DocumentRepository(SecuraDbContext context) : base(context) { }

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
