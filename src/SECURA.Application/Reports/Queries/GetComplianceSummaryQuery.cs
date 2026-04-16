using MediatR;
using SECURA.Application.Common.Interfaces;
using SECURA.Application.Reports.DTOs;
using SECURA.Domain.Enums;

namespace SECURA.Application.Reports.Queries;

public sealed record GetComplianceSummaryQuery : IRequest<ComplianceSummaryDto>;

public sealed class GetComplianceSummaryQueryHandler
    : IRequestHandler<GetComplianceSummaryQuery, ComplianceSummaryDto>
{
    private readonly IProducerRepository _producers;

    public GetComplianceSummaryQueryHandler(IProducerRepository producers)
    {
        _producers = producers;
    }

    public async Task<ComplianceSummaryDto> Handle(
        GetComplianceSummaryQuery request, CancellationToken cancellationToken)
    {
        var (allProducers, totalProducers) = await _producers.GetPagedAsync(
            1, int.MaxValue, cancellationToken: cancellationToken);

        var exp30 = await _producers.GetExpiringLicensesAsync(30, cancellationToken);
        var exp60 = await _producers.GetExpiringLicensesAsync(60, cancellationToken);
        var exp90 = await _producers.GetExpiringLicensesAsync(90, cancellationToken);

        var allLicenses = allProducers.SelectMany(p => p.Licenses).ToList();
        var activeLicenses = allLicenses.Count(l => l.Status == LicenseStatus.Active && !l.IsExpired);
        var expiredLicenses = allLicenses.Count(l => l.IsExpired);

        return new ComplianceSummaryDto
        {
            TotalProducers = totalProducers,
            ActiveLicenses = activeLicenses,
            ExpiredLicenses = expiredLicenses,
            ExpiringIn30Days = exp30.Count,
            ExpiringIn60Days = exp60.Count,
            ExpiringIn90Days = exp90.Count,
            GeneratedAt = DateTime.UtcNow
        };
    }
}
