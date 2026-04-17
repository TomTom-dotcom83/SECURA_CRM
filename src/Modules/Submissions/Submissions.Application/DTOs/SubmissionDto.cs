using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Submissions.Application.DTOs;

public sealed class SubmissionDto
{
    public Guid Id { get; init; }
    public Guid AgencyId { get; init; }
    public string AgencyName { get; init; } = string.Empty;
    public LobType Lob { get; init; }
    public string State { get; init; } = string.Empty;
    public SubmissionStatus Status { get; init; }
    public string StatusDisplay => Status.ToString();
    public DateTime ReceivedDate { get; init; }
    public string? InsuredName { get; init; }
    public string? Description { get; init; }
    public string? PolicyRef { get; init; }
    public string? QuoteNumber { get; init; }
    public decimal? QuotedPremium { get; init; }
    public string? DeclineReason { get; init; }
    public DateTime SlaDeadline { get; init; }
    public bool IsOverdue { get; init; }
    public List<UWNoteDto> UWNotes { get; init; } = [];
    public DateTime CreatedAt { get; init; }
}

public sealed class SubmissionSummaryDto
{
    public Guid Id { get; init; }
    public string AgencyName { get; init; } = string.Empty;
    public LobType Lob { get; init; }
    public string State { get; init; } = string.Empty;
    public SubmissionStatus Status { get; init; }
    public DateTime ReceivedDate { get; init; }
    public string? InsuredName { get; init; }
    public DateTime SlaDeadline { get; init; }
    public bool IsOverdue { get; init; }
}

public sealed class UWNoteDto
{
    public Guid Id { get; init; }
    public string AuthorUserId { get; init; } = string.Empty;
    public string? AuthorDisplayName { get; init; }
    public string NoteText { get; init; } = string.Empty;
    public DateTime CreatedDate { get; init; }
}
