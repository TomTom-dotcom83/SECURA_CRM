using MediatR;
using Secura.DistributionCrm.Documents.Application.Abstractions;
using Secura.DistributionCrm.Documents.Application.DTOs;

namespace Secura.DistributionCrm.Documents.Application.Queries;

public sealed record GetDocumentsQuery(
    string RelatedEntityType,
    Guid RelatedEntityId) : IRequest<IReadOnlyList<DocumentMetadataDto>>;

public sealed class GetDocumentsQueryHandler
    : IRequestHandler<GetDocumentsQuery, IReadOnlyList<DocumentMetadataDto>>
{
    private readonly IDocumentRepository _documents;

    public GetDocumentsQueryHandler(IDocumentRepository documents)
    {
        _documents = documents;
    }

    public async Task<IReadOnlyList<DocumentMetadataDto>> Handle(
        GetDocumentsQuery request, CancellationToken cancellationToken)
    {
        var docs = await _documents.GetByRelatedEntityAsync(
            request.RelatedEntityType, request.RelatedEntityId, cancellationToken);

        return docs.Select(d => new DocumentMetadataDto
        {
            Id = d.Id,
            RelatedEntityType = d.RelatedEntityType,
            RelatedEntityId = d.RelatedEntityId,
            DocumentType = d.DocumentType,
            FileName = d.FileName,
            StorageRef = d.StorageRef,
            FileSizeBytes = d.FileSizeBytes,
            ContentType = d.ContentType,
            UploadedByUserId = d.UploadedByUserId,
            UploadedAt = d.UploadedAt,
            Description = d.Description,
            IsActive = d.IsActive
        }).ToList();
    }
}
