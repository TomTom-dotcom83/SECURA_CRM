using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Secura.DistributionCrm.Claims.Domain;

namespace Secura.DistributionCrm.Claims.Infrastructure.Persistence.Configurations;

public sealed class ClaimReferenceConfiguration : IEntityTypeConfiguration<ClaimReference>
{
    public void Configure(EntityTypeBuilder<ClaimReference> builder)
    {
        builder.ToTable("ClaimReferences");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.ExternalClaimNumber).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Lob).IsRequired().HasConversion<string>().HasMaxLength(100);
        builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(c => c.InsuredName).HasMaxLength(200);
        builder.Property(c => c.Description).HasMaxLength(2000);
        builder.Property(c => c.AssignedAdjusterUserId).HasMaxLength(100);
        builder.Property(c => c.ReserveAmount).HasPrecision(18, 2);
        builder.Property(c => c.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(c => c.ModifiedBy).HasMaxLength(100);
        builder.HasIndex(c => c.AgencyId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.ExternalClaimNumber).IsUnique();
        builder.Ignore(c => c.DomainEvents);
    }
}
