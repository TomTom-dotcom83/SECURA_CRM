using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SECURA.Domain.Entities;

namespace SECURA.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).ValueGeneratedNever();
        builder.Property(o => o.Type).IsRequired().HasMaxLength(500);
        builder.Property(o => o.Payload).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(o => o.Error).HasMaxLength(2000);
        builder.HasIndex(o => o.ProcessedAt);
        builder.HasIndex(o => o.OccurredOn);
        builder.Ignore(o => o.IsProcessed);
        builder.Ignore(o => o.DomainEvents);
    }
}
