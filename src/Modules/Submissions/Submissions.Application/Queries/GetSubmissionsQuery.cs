using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Application.Models;
using Secura.DistributionCrm.Submissions.Application.Abstractions;
using Secura.DistributionCrm.Submissions.Application.DTOs;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Submissions.Application.Queries;

public sealed record GetSubmissionsQuery(
    int Page = 1,
    int PageSize = 25,
    SubmissionStatus? Status = null,
    LobType? Lob = null,
    string? State = null,
    Guid? AgencyId = null,
    bool? IsOverdue = null) : IRequest<PagedResult<SubmissionSummaryDto>>;

public sealed class GetSubmissionsQueryHandler
    : IRequestHandler<GetSubmissionsQuery, PagedResult<SubmissionSummaryDto>>
{
    private readonly ISubmissionRepository _submissions;

    public GetSubmissionsQueryHandler(ISubmissionRepository submissions)
    {
        _submissions = submissions;
    }

    public async Task<PagedResult<SubmissionSummaryDto>> Handle(
        GetSubmissionsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _submissions.GetPagedAsync(
            request.Page, request.PageSize,
            request.Status, request.Lob, request.State,
            request.AgencyId, request.IsOverdue,
            cancellationToken);

        var dtos = items.Select(s => new SubmissionSummaryDto
        {
            Id = s.Id,
            Lob = s.Lob,
            State = s.State,
            Status = s.Status,
            ReceivedDate = s.ReceivedDate,
            InsuredName = s.InsuredName,
            SlaDeadline = s.SlaDeadline,
            IsOverdue = s.IsOverdue
        }).ToList();

        return PagedResult<SubmissionSummaryDto>.Create(dtos, total, request.Page, request.PageSize);
    }
}
