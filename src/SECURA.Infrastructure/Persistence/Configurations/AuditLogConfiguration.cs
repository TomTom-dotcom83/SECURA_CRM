using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SECURA.Domain.Entities;

namespace SECURA.Infrastructure.Persistence.Configurations;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();
        builder.Property(a => a.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(a => a.EntityId).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Action).IsRequired().HasMaxLength(50);
        builder.Property(a => a.UserId).IsRequired().HasMaxLength(100);
        builder.Property(a => a.UserDisplayName).HasMaxLength(200);
        builder.Property(a => a.IpAddress).HasMaxLength(50);
        builder.Property(a => a.CorrelationId).HasMaxLength(100);
        builder.Property(a => a.BeforeJson).HasColumnType("nvarchar(max)");
        builder.Property(a => a.AfterJson).HasColumnType("nvarchar(max)");
        builder.HasIndex(a => new { a.EntityType, a.EntityId });
        builder.HasIndex(a => a.Timestamp);
        builder.HasIndex(a => a.UserId);
        builder.Ignore(a => a.DomainEvents);
    }
}
