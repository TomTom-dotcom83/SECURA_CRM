using SECURA.Domain.Common;

namespace SECURA.Domain.Entities;

public sealed class DocumentMetadata : Entity<Guid>, IAuditableEntity
{
    private DocumentMetadata() { }

    private DocumentMetadata(Guid id, string relatedEntityType, Guid relatedEntityId,
        string documentType, string storageRef, string uploadedByUserId) : base(id)
    {
        RelatedEntityType = relatedEntityType;
        RelatedEntityId = relatedEntityId;
        DocumentType = documentType;
        StorageRef = storageRef;
        UploadedByUserId = uploadedByUserId;
        UploadedAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = uploadedByUserId;
    }

    public string RelatedEntityType { get; private set; } = string.Empty;
    public Guid RelatedEntityId { get; private set; }
    public string DocumentType { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string StorageRef { get; private set; } = string.Empty;
    public long? FileSizeBytes { get; private set; }
    public string? ContentType { get; private set; }
    public string UploadedByUserId { get; private set; } = string.Empty;
    public DateTime UploadedAt { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    public static DocumentMetadata Create(string relatedEntityType, Guid relatedEntityId,
        string documentType, string fileName, string storageRef, string uploadedByUserId)
    {
        if (string.IsNullOrWhiteSpace(storageRef))
            throw new DomainException("Storage reference is required.");

        return new DocumentMetadata(Guid.NewGuid(), relatedEntityType, relatedEntityId,
            documentType, storageRef, uploadedByUserId)
        {
            FileName = fileName
        };
    }
}
