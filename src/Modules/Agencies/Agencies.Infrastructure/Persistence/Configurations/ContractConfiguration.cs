using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Secura.DistributionCrm.Agencies.Domain.Agencies;

namespace Secura.DistributionCrm.Agencies.Infrastructure.Persistence.Configurations;

public sealed class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.CommissionScheduleRef).IsRequired().HasMaxLength(200);
        builder.Property(c => c.ContractNumber).HasMaxLength(100);
        builder.Property(c => c.Notes).HasMaxLength(4000);
        builder.Property(c => c.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(c => c.ModifiedBy).HasMaxLength(100);

        builder.HasIndex(c => c.AgencyId);
        builder.HasIndex(c => c.EffectiveDate);
        builder.HasIndex(c => c.TerminationDate);

        builder.Ignore(c => c.IsActive);
        builder.Ignore(c => c.DomainEvents);
    }
}
