using SECURA.Domain.Common;
using SECURA.Domain.Enums;
using SECURA.Domain.Events;

namespace SECURA.Domain.Entities;

public sealed class Submission : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<UWNote> _uwNotes = [];
    private readonly List<AppetiteTag> _appetiteTags = [];

    // Submission state machine allowed transitions
    private static readonly Dictionary<SubmissionStatus, SubmissionStatus[]> AllowedTransitions = new()
    {
        [SubmissionStatus.New]      = [SubmissionStatus.Triaged, SubmissionStatus.Declined],
        [SubmissionStatus.Triaged]  = [SubmissionStatus.InReview, SubmissionStatus.Declined],
        [SubmissionStatus.InReview] = [SubmissionStatus.Referred, SubmissionStatus.Quoted, SubmissionStatus.Declined],
        [SubmissionStatus.Referred] = [SubmissionStatus.InReview, SubmissionStatus.Declined],
        [SubmissionStatus.Quoted]   = [SubmissionStatus.Bound, SubmissionStatus.Declined],
        [SubmissionStatus.Bound]    = [],
        [SubmissionStatus.Declined] = []
    };

    // SLA hours by LOB (configurable via AppSettings in a real system)
    private static readonly Dictionary<LobType, int> SlaHours = new()
    {
        [LobType.CommercialAuto]       = 48,
        [LobType.GeneralLiability]     = 72,
        [LobType.CommercialProperty]   = 72,
        [LobType.WorkersCompensation]  = 96,
        [LobType.CommercialUmbrella]   = 120,
        [LobType.ProfessionalLiability]= 120,
        [LobType.CyberLiability]       = 72,
        [LobType.BOP]                  = 48,
    };

    private Submission() { }

    private Submission(Guid id, Guid agencyId, LobType lob, string state,
        DateTime receivedDate) : base(id)
    {
        AgencyId = agencyId;
        Lob = lob;
        State = state.ToUpperInvariant();
        ReceivedDate = receivedDate;
        Status = SubmissionStatus.New;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = string.Empty;
    }

    public Guid AgencyId { get; private set; }
    public LobType Lob { get; private set; }
    public string State { get; private set; } = string.Empty;
    public SubmissionStatus Status { get; private set; }
    public DateTime ReceivedDate { get; private set; }
    public string? PolicyRef { get; private set; }
    public string? InsuredName { get; private set; }
    public string? Description { get; private set; }
    public string? ReferredToUserId { get; private set; }
    public DateTime? ReferredAt { get; private set; }
    public string? QuoteNumber { get; private set; }
    public decimal? QuotedPremium { get; private set; }
    public string? DeclineReason { get; private set; }

    public DateTime SlaDeadline => ReceivedDate.AddHours(
        SlaHours.TryGetValue(Lob, out var hours) ? hours : 72);

    public bool IsOverdue => Status is not (SubmissionStatus.Bound or SubmissionStatus.Declined)
                             && DateTime.UtcNow > SlaDeadline;

    public IReadOnlyList<UWNote> UWNotes => _uwNotes.AsReadOnly();
    public IReadOnlyList<AppetiteTag> AppetiteTags => _appetiteTags.AsReadOnly();

    public Agency? Agency { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    public static Submission Create(Guid agencyId, LobType lob, string state,
        DateTime receivedDate, string? insuredName = null)
    {
        if (string.IsNullOrWhiteSpace(state))
            throw new DomainException("Submission state is required.");

        var submission = new Submission(Guid.NewGuid(), agencyId, lob, state, receivedDate)
        {
            InsuredName = insuredName
        };

        submission.AddDomainEvent(
            new SubmissionStatusChangedEvent(submission.Id, null, SubmissionStatus.New));
        return submission;
    }

    public bool CanTransitionTo(SubmissionStatus targetStatus) =>
        AllowedTransitions.TryGetValue(Status, out var allowed)
        && allowed.Contains(targetStatus);

    public void Transition(SubmissionStatus targetStatus, string userId)
    {
        if (!CanTransitionTo(targetStatus))
            throw new DomainException(
                $"Cannot transition submission from {Status} to {targetStatus}.");

        var previous = Status;
        Status = targetStatus;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;

        AddDomainEvent(new SubmissionStatusChangedEvent(Id, previous, targetStatus));
    }

    public void Refer(string toUserId, string userId)
    {
        Transition(SubmissionStatus.Referred, userId);
        ReferredToUserId = toUserId;
        ReferredAt = DateTime.UtcNow;
    }

    public void Quote(string quoteNumber, decimal premium, string userId)
    {
        Transition(SubmissionStatus.Quoted, userId);
        QuoteNumber = quoteNumber;
        QuotedPremium = premium;
    }

    public void Decline(string reason, string userId)
    {
        Transition(SubmissionStatus.Declined, userId);
        DeclineReason = reason;
    }

    public UWNote AddNote(string text, string authorUserId)
    {
        var note = UWNote.Create(Id, authorUserId, text);
        _uwNotes.Add(note);
        return note;
    }

    public void SetPolicyRef(string policyRef) => PolicyRef = policyRef;

    public void AddAppetiteTag(AppetiteTag tag)
    {
        if (!_appetiteTags.Contains(tag))
            _appetiteTags.Add(tag);
    }
}
