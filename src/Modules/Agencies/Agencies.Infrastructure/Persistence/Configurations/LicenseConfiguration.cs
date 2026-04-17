using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Secura.DistributionCrm.Agencies.Domain.Producers;

namespace Secura.DistributionCrm.Agencies.Infrastructure.Persistence.Configurations;

public sealed class LicenseConfiguration : IEntityTypeConfiguration<License>
{
    public void Configure(EntityTypeBuilder<License> builder)
    {
        builder.ToTable("Licenses");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();

        builder.Property(l => l.State).IsRequired().HasMaxLength(2);
        builder.Property(l => l.Lob).IsRequired().HasConversion<string>().HasMaxLength(100);
        builder.Property(l => l.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(l => l.LicenseNumber).HasMaxLength(50);
        builder.Property(l => l.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(l => l.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(l => l.ProducerId);
        builder.HasIndex(l => l.State);
        builder.HasIndex(l => l.Status);
        builder.HasIndex(l => l.ExpirationDate);

        builder.Ignore(l => l.IsExpired);
        builder.Ignore(l => l.DomainEvents);
    }
}
