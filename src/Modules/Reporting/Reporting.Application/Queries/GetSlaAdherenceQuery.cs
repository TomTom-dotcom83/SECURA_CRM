using MediatR;
using Secura.DistributionCrm.Submissions.Application.Abstractions;

namespace Secura.DistributionCrm.Reporting.Application.Queries;

public sealed record GetSlaAdherenceQuery : IRequest<SlaAdherenceDto>;

public sealed record SlaAdherenceDto(
    int TotalOpen,
    int OnTime,
    int Overdue,
    decimal AdherencePercent,
    DateTime GeneratedAt);

public sealed class GetSlaAdherenceQueryHandler
    : IRequestHandler<GetSlaAdherenceQuery, SlaAdherenceDto>
{
    private readonly ISubmissionRepository _submissions;

    public GetSlaAdherenceQueryHandler(ISubmissionRepository submissions)
    {
        _submissions = submissions;
    }

    public async Task<SlaAdherenceDto> Handle(
        GetSlaAdherenceQuery request, CancellationToken cancellationToken)
    {
        var (total, onTime) = await _submissions.GetSlaStatsAsync(cancellationToken);
        var overdue = total - onTime;
        var adherence = total == 0 ? 100m : Math.Round((decimal)onTime / total * 100, 1);

        return new SlaAdherenceDto(total, onTime, overdue, adherence, DateTime.UtcNow);
    }
}
