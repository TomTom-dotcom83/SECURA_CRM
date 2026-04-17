using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Secura.DistributionCrm.Relationships.Domain.Interactions;

namespace Secura.DistributionCrm.Relationships.Infrastructure.Persistence.Configurations;

public sealed class InteractionConfiguration : IEntityTypeConfiguration<Interaction>
{
    public void Configure(EntityTypeBuilder<Interaction> builder)
    {
        builder.ToTable("Interactions");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();
        builder.Property(i => i.RelatedEntityType).IsRequired().HasMaxLength(100);
        builder.Property(i => i.InteractionType).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(i => i.Summary).IsRequired().HasMaxLength(500);
        builder.Property(i => i.DetailNotes).HasMaxLength(4000);
        builder.Property(i => i.CreatedByUserId).IsRequired().HasMaxLength(100);
        builder.Property(i => i.CreatedByDisplayName).HasMaxLength(200);
        builder.Property(i => i.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(i => i.ModifiedBy).HasMaxLength(100);
        builder.HasIndex(i => new { i.RelatedEntityType, i.RelatedEntityId });
        builder.HasIndex(i => i.Timestamp);
        builder.Ignore(i => i.DomainEvents);
    }
}
