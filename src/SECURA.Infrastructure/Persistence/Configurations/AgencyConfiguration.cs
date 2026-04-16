using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SECURA.Domain.Entities;
using SECURA.Domain.ValueObjects;

namespace SECURA.Infrastructure.Persistence.Configurations;

public sealed class AgencyConfiguration : IEntityTypeConfiguration<Agency>
{
    public void Configure(EntityTypeBuilder<Agency> builder)
    {
        builder.ToTable("Agencies");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Name).IsRequired().HasMaxLength(200);
        builder.Property(a => a.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(a => a.Tier).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(a => a.PrimaryState).IsRequired().HasMaxLength(2);
        builder.Property(a => a.Phone).HasMaxLength(20);
        builder.Property(a => a.Email).HasMaxLength(255);
        builder.Property(a => a.Website).HasMaxLength(500);
        builder.Property(a => a.Notes).HasMaxLength(4000);
        builder.Property(a => a.TaxId).HasMaxLength(20);
        builder.Property(a => a.FeinNumber).HasMaxLength(20);
        builder.Property(a => a.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(a => a.ModifiedBy).HasMaxLength(100);

        // Address owned entity
        builder.OwnsOne(a => a.PhysicalAddress, addr =>
        {
            addr.Property(x => x.Line1).HasColumnName("Address_Line1").HasMaxLength(200);
            addr.Property(x => x.Line2).HasColumnName("Address_Line2").HasMaxLength(200);
            addr.Property(x => x.City).HasColumnName("Address_City").HasMaxLength(100);
            addr.Property(x => x.State).HasColumnName("Address_State").HasMaxLength(2);
            addr.Property(x => x.Zip).HasColumnName("Address_Zip").HasMaxLength(10);
            addr.Property(x => x.Country).HasColumnName("Address_Country").HasMaxLength(3);
        });

        builder.HasMany(a => a.Branches)
            .WithOne(b => b.Agency)
            .HasForeignKey(b => b.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Contracts)
            .WithOne(c => c.Agency)
            .HasForeignKey(c => c.AgencyId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.Tier);
        builder.HasIndex(a => a.PrimaryState);
        builder.HasIndex(a => a.Name);

        // Ignore domain events (not persisted directly)
        builder.Ignore(a => a.DomainEvents);
    }
}
