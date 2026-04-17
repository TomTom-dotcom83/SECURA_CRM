using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Secura.DistributionCrm.Documents.Domain;

namespace Secura.DistributionCrm.Documents.Infrastructure.Persistence.Configurations;

public sealed class DocumentMetadataConfiguration : IEntityTypeConfiguration<DocumentMetadata>
{
    public void Configure(EntityTypeBuilder<DocumentMetadata> builder)
    {
        builder.ToTable("DocumentMetadata");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();
        builder.Property(d => d.RelatedEntityType).IsRequired().HasMaxLength(100);
        builder.Property(d => d.DocumentType).IsRequired().HasMaxLength(100);
        builder.Property(d => d.FileName).HasMaxLength(500);
        builder.Property(d => d.StorageRef).IsRequired().HasMaxLength(1000);
        builder.Property(d => d.ContentType).HasMaxLength(200);
        builder.Property(d => d.Description).HasMaxLength(1000);
        builder.Property(d => d.UploadedByUserId).IsRequired().HasMaxLength(100);
        builder.Property(d => d.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(d => d.ModifiedBy).HasMaxLength(100);
        builder.HasIndex(d => new { d.RelatedEntityType, d.RelatedEntityId });
        builder.HasIndex(d => d.IsActive);
        builder.Ignore(d => d.DomainEvents);
    }
}
