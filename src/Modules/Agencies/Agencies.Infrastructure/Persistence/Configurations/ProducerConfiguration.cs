using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Secura.DistributionCrm.Agencies.Domain.Producers;

namespace Secura.DistributionCrm.Agencies.Infrastructure.Persistence.Configurations;

public sealed class ProducerConfiguration : IEntityTypeConfiguration<Producer>
{
    public void Configure(EntityTypeBuilder<Producer> builder)
    {
        builder.ToTable("Producers");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Npn)
            .HasConversion(npn => npn.Value, value => NationalProducerNumber.Create(value))
            .IsRequired()
            .HasMaxLength(20)
            .HasColumnName("NPN");

        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Email).HasMaxLength(255);
        builder.Property(p => p.Phone).HasMaxLength(20);
        builder.Property(p => p.LicenseStatus).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(p => p.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(p => p.ModifiedBy).HasMaxLength(100);

        builder.HasMany(p => p.Licenses)
            .WithOne(l => l.Producer)
            .HasForeignKey(l => l.ProducerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.Npn).IsUnique();
        builder.HasIndex(p => p.BranchId);
        builder.HasIndex(p => p.ActiveFlag);
        builder.HasIndex(p => p.LicenseStatus);

        builder.Ignore(p => p.DomainEvents);
        builder.Ignore(p => p.FullName);
    }
}
