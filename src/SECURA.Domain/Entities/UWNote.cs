using SECURA.Domain.Common;

namespace SECURA.Domain.Entities;

public sealed class UWNote : Entity<Guid>
{
    private UWNote() { }

    private UWNote(Guid id, Guid submissionId, string authorUserId, string noteText) : base(id)
    {
        SubmissionId = submissionId;
        AuthorUserId = authorUserId;
        NoteText = noteText;
        CreatedDate = DateTime.UtcNow;
    }

    public Guid SubmissionId { get; private set; }
    public string AuthorUserId { get; private set; } = string.Empty;
    public string AuthorDisplayName { get; private set; } = string.Empty;
    public string NoteText { get; private set; } = string.Empty;
    public DateTime CreatedDate { get; private set; }
    public bool IsInternal { get; private set; } = true;

    public Submission? Submission { get; private set; }

    public static UWNote Create(Guid submissionId, string authorUserId, string noteText)
    {
        if (string.IsNullOrWhiteSpace(noteText))
            throw new DomainException("Note text cannot be empty.");

        return new UWNote(Guid.NewGuid(), submissionId, authorUserId, noteText.Trim());
    }
}
