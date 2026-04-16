using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SECURA.Domain.Entities;

namespace SECURA.Infrastructure.Persistence.Configurations;

public sealed class OnboardingChecklistConfiguration : IEntityTypeConfiguration<OnboardingChecklist>
{
    public void Configure(EntityTypeBuilder<OnboardingChecklist> builder)
    {
        builder.ToTable("OnboardingChecklists");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();
        builder.Property(c => c.TemplateName).IsRequired().HasMaxLength(200);
        builder.Property(c => c.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(c => c.ModifiedBy).HasMaxLength(100);
        builder.HasMany(c => c.Items)
            .WithOne(i => i.Checklist)
            .HasForeignKey(i => i.ChecklistId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(c => c.AgencyId);
        builder.Ignore(c => c.IsComplete);
        builder.Ignore(c => c.DomainEvents);
    }
}

public sealed class OnboardingChecklistItemConfiguration : IEntityTypeConfiguration<OnboardingChecklistItem>
{
    public void Configure(EntityTypeBuilder<OnboardingChecklistItem> builder)
    {
        builder.ToTable("OnboardingChecklistItems");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedNever();
        builder.Property(i => i.StepName).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Description).HasMaxLength(1000);
        builder.Property(i => i.CompletedByUserId).HasMaxLength(100);
        builder.HasIndex(i => i.ChecklistId);
        builder.Ignore(i => i.DomainEvents);
    }
}
