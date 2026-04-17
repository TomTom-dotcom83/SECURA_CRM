using MediatR;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.Submissions.Application.Abstractions;
using Secura.DistributionCrm.Reporting.Application.DTOs;

namespace Secura.DistributionCrm.Reporting.Application.Queries;

public sealed record GetAgencyScorecardQuery(Guid AgencyId) : IRequest<AgencyScorecardDto>;

public sealed class GetAgencyScorecardQueryHandler
    : IRequestHandler<GetAgencyScorecardQuery, AgencyScorecardDto>
{
    private readonly IAgencyRepository _agencies;
    private readonly ISubmissionRepository _submissions;
    private readonly IProducerRepository _producers;

    public GetAgencyScorecardQueryHandler(
        IAgencyRepository agencies,
        ISubmissionRepository submissions,
        IProducerRepository producers)
    {
        _agencies = agencies;
        _submissions = submissions;
        _producers = producers;
    }

    public async Task<AgencyScorecardDto> Handle(
        GetAgencyScorecardQuery request, CancellationToken cancellationToken)
    {
        var agency = await _agencies.GetWithDetailsAsync(request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Agency {request.AgencyId} not found.");

        var (total, bound, declined) = await _submissions.GetAgencySubmissionStatsAsync(
            request.AgencyId, cancellationToken);

        var (producers, _) = await _producers.GetPagedAsync(
            1, int.MaxValue, agencyId: request.AgencyId,
            activeOnly: true, cancellationToken: cancellationToken);

        var expiringLicenses = await _producers.GetLicensesByAgencyAsync(
            request.AgencyId, cancellationToken);

        return new AgencyScorecardDto
        {
            AgencyId = agency.Id,
            AgencyName = agency.Name,
            Tier = agency.Tier,
            TotalSubmissions = total,
            BoundSubmissions = bound,
            DeclinedSubmissions = declined,
            ActiveProducers = producers.Count,
            ExpiringLicenses = expiringLicenses.Count(l => l.ExpiresWithinDays(30)),
            OpenClaims = 0,
            AvgDaysToOnboard = 0,
            GeneratedAt = DateTime.UtcNow
        };
    }
}
