using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Secura.DistributionCrm.Submissions.Domain;

namespace Secura.DistributionCrm.Submissions.Infrastructure.Persistence.Configurations;

public sealed class AppetiteTagConfiguration : IEntityTypeConfiguration<AppetiteTag>
{
    public void Configure(EntityTypeBuilder<AppetiteTag> builder)
    {
        builder.ToTable("AppetiteTags");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Label).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Lob).HasConversion<string>().HasMaxLength(100);
        builder.Property(t => t.State).HasMaxLength(2);
        builder.Property(t => t.Color).HasMaxLength(20);
        builder.HasIndex(t => t.IsActive);
        builder.Ignore(t => t.DomainEvents);
    }
}
