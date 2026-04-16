using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SECURA.Domain.Entities;

namespace SECURA.Infrastructure.Persistence.Configurations;

public sealed class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id).ValueGeneratedNever();

        builder.Property(b => b.Name).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Phone).HasMaxLength(20);
        builder.Property(b => b.Email).HasMaxLength(255);
        builder.Property(b => b.State).HasMaxLength(2);
        builder.Property(b => b.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(b => b.ModifiedBy).HasMaxLength(100);

        builder.HasMany(b => b.Producers)
            .WithOne(p => p.Branch)
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(b => b.AgencyId);
        builder.HasIndex(b => b.IsActive);

        builder.Ignore(b => b.DomainEvents);
    }
}
