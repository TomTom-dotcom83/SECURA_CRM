using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Secura.DistributionCrm.Submissions.Domain;

namespace Secura.DistributionCrm.Submissions.Infrastructure.Persistence.Configurations;

public sealed class UWNoteConfiguration : IEntityTypeConfiguration<UWNote>
{
    public void Configure(EntityTypeBuilder<UWNote> builder)
    {
        builder.ToTable("UWNotes");
        builder.HasKey(n => n.Id);
        builder.Property(n => n.Id).ValueGeneratedNever();
        builder.Property(n => n.AuthorUserId).IsRequired().HasMaxLength(100);
        builder.Property(n => n.AuthorDisplayName).HasMaxLength(200);
        builder.Property(n => n.NoteText).IsRequired().HasMaxLength(4000);
        builder.HasIndex(n => n.SubmissionId);
        builder.Ignore(n => n.DomainEvents);
    }
}
