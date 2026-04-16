using MediatR;
using SECURA.Application.Common.Interfaces;
using SECURA.Application.Reports.DTOs;
using SECURA.Domain.Enums;

namespace SECURA.Application.Reports.Queries;

public sealed record GetPipelineReportQuery : IRequest<PipelineReportDto>;

public sealed class GetPipelineReportQueryHandler
    : IRequestHandler<GetPipelineReportQuery, PipelineReportDto>
{
    private readonly ISubmissionRepository _submissions;

    public GetPipelineReportQueryHandler(ISubmissionRepository submissions)
    {
        _submissions = submissions;
    }

    public async Task<PipelineReportDto> Handle(
        GetPipelineReportQuery request, CancellationToken cancellationToken)
    {
        var counts = await _submissions.GetStatusCountsAsync(cancellationToken);
        var (total, onTime) = await _submissions.GetSlaStatsAsync(cancellationToken);

        var overdue = total - onTime;
        var slaAdherence = total == 0 ? 100m : Math.Round((decimal)onTime / total * 100, 1);

        return new PipelineReportDto
        {
            NewCount = counts.GetValueOrDefault(SubmissionStatus.New),
            TriagedCount = counts.GetValueOrDefault(SubmissionStatus.Triaged),
            InReviewCount = counts.GetValueOrDefault(SubmissionStatus.InReview),
            ReferredCount = counts.GetValueOrDefault(SubmissionStatus.Referred),
            QuotedCount = counts.GetValueOrDefault(SubmissionStatus.Quoted),
            BoundCount = counts.GetValueOrDefault(SubmissionStatus.Bound),
            DeclinedCount = counts.GetValueOrDefault(SubmissionStatus.Declined),
            OverdueCount = overdue,
            SlaAdherencePercent = slaAdherence,
            GeneratedAt = DateTime.UtcNow
        };
    }
}
