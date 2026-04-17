namespace Secura.DistributionCrm.Documents.Application.DTOs;

public sealed class DocumentMetadataDto
{
    public Guid Id { get; init; }
    public string RelatedEntityType { get; init; } = string.Empty;
    public Guid RelatedEntityId { get; init; }
    public string DocumentType { get; init; } = string.Empty;
    public string FileName { get; init; } = string.Empty;
    public string StorageRef { get; init; } = string.Empty;
    public long? FileSizeBytes { get; init; }
    public string? ContentType { get; init; }
    public string UploadedByUserId { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
}
