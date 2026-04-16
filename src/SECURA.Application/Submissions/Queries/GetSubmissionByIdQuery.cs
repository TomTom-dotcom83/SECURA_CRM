using MediatR;
using SECURA.Application.Common.Interfaces;
using SECURA.Application.Submissions.DTOs;
using SECURA.Domain.Common;

namespace SECURA.Application.Submissions.Queries;

public sealed record GetSubmissionByIdQuery(Guid Id) : IRequest<SubmissionDto>;

public sealed class GetSubmissionByIdQueryHandler : IRequestHandler<GetSubmissionByIdQuery, SubmissionDto>
{
    private readonly ISubmissionRepository _submissions;

    public GetSubmissionByIdQueryHandler(ISubmissionRepository submissions)
    {
        _submissions = submissions;
    }

    public async Task<SubmissionDto> Handle(GetSubmissionByIdQuery request, CancellationToken cancellationToken)
    {
        var submission = await _submissions.GetWithNotesAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Submission {request.Id} not found.");

        return new SubmissionDto
        {
            Id = submission.Id,
            AgencyId = submission.AgencyId,
            AgencyName = submission.Agency?.Name ?? string.Empty,
            Lob = submission.Lob,
            State = submission.State,
            Status = submission.Status,
            ReceivedDate = submission.ReceivedDate,
            InsuredName = submission.InsuredName,
            PolicyRef = submission.PolicyRef,
            QuoteNumber = submission.QuoteNumber,
            QuotedPremium = submission.QuotedPremium,
            DeclineReason = submission.DeclineReason,
            SlaDeadline = submission.SlaDeadline,
            IsOverdue = submission.IsOverdue,
            CreatedAt = submission.CreatedAt,
            UWNotes = submission.UWNotes.Select(n => new UWNoteDto
            {
                Id = n.Id,
                AuthorUserId = n.AuthorUserId,
                NoteText = n.NoteText,
                CreatedDate = n.CreatedDate
            }).ToList()
        };
    }
}
