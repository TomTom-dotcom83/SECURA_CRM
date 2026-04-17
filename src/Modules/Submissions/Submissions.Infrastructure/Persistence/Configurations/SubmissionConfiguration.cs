using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Secura.DistributionCrm.Submissions.Domain;

namespace Secura.DistributionCrm.Submissions.Infrastructure.Persistence.Configurations;

public sealed class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("Submissions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();

        builder.Property(s => s.Lob).IsRequired().HasConversion<string>().HasMaxLength(100);
        builder.Property(s => s.State).IsRequired().HasMaxLength(2);
        builder.Property(s => s.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
        builder.Property(s => s.InsuredName).HasMaxLength(200);
        builder.Property(s => s.Description).HasMaxLength(4000);
        builder.Property(s => s.PolicyRef).HasMaxLength(100);
        builder.Property(s => s.QuoteNumber).HasMaxLength(100);
        builder.Property(s => s.DeclineReason).HasMaxLength(1000);
        builder.Property(s => s.ReferredToUserId).HasMaxLength(100);
        builder.Property(s => s.QuotedPremium).HasPrecision(18, 2);
        builder.Property(s => s.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(s => s.ModifiedBy).HasMaxLength(100);

        builder.HasMany(s => s.UWNotes)
            .WithOne(n => n.Submission)
            .HasForeignKey(n => n.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.AppetiteTags)
            .WithMany()
            .UsingEntity("SubmissionAppetiteTags");

        builder.HasIndex(s => s.AgencyId);
        builder.HasIndex(s => s.Status);
        builder.HasIndex(s => s.State);
        builder.HasIndex(s => s.Lob);
        builder.HasIndex(s => s.ReceivedDate);

        builder.Ignore(s => s.DomainEvents);
        builder.Ignore(s => s.SlaDeadline);
        builder.Ignore(s => s.IsOverdue);
    }
}
