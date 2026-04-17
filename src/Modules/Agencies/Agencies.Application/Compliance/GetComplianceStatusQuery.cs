using MediatR;
using Secura.DistributionCrm.Agencies.Application.Abstractions;

namespace Secura.DistributionCrm.Agencies.Application.Compliance;

public sealed record GetComplianceStatusQuery(Guid AgencyId) : IRequest<ComplianceStatusDto>;

public sealed class GetComplianceStatusQueryHandler
    : IRequestHandler<GetComplianceStatusQuery, ComplianceStatusDto>
{
    private readonly IAgencyRepository _agencies;
    private readonly IProducerRepository _producers;
    private readonly IChecklistRepository _checklists;

    public GetComplianceStatusQueryHandler(
        IAgencyRepository agencies,
        IProducerRepository producers,
        IChecklistRepository checklists)
    {
        _agencies = agencies;
        _producers = producers;
        _checklists = checklists;
    }

    public async Task<ComplianceStatusDto> Handle(
        GetComplianceStatusQuery request, CancellationToken cancellationToken)
    {
        var agency = await _agencies.GetWithDetailsAsync(request.AgencyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Agency {request.AgencyId} not found.");

        var licenses = await _producers.GetLicensesByAgencyAsync(
            request.AgencyId, cancellationToken);

        var activeLicenses = licenses.Count(l => !l.IsExpired);
        var expiredLicenses = licenses.Count(l => l.IsExpired);
        var expiring30 = licenses.Count(l => l.ExpiresWithinDays(30) && !l.IsExpired);
        var expiring60 = licenses.Count(l => l.ExpiresWithinDays(60) && !l.IsExpired);
        var expiring90 = licenses.Count(l => l.ExpiresWithinDays(90) && !l.IsExpired);

        var checklist = await _checklists.GetByAgencyIdAsync(request.AgencyId, cancellationToken);
        var checklistTotal = checklist?.Items.Count ?? 0;
        var checklistCompleted = checklist?.Items.Count(i => i.IsCompleted) ?? 0;
        var checklistComplete = checklist?.IsComplete ?? false;

        var licenseScore = licenses.Count > 0
            ? (double)activeLicenses / licenses.Count * 100.0
            : 100.0;
        var checklistScore = checklistTotal > 0
            ? (double)checklistCompleted / checklistTotal * 100.0
            : 100.0;
        var complianceScore = Math.Round(licenseScore * 0.6 + checklistScore * 0.4, 1);

        return new ComplianceStatusDto
        {
            AgencyId = agency.Id,
            AgencyName = agency.Name,
            TotalLicenses = licenses.Count,
            ActiveLicenses = activeLicenses,
            ExpiredLicenses = expiredLicenses,
            ExpiringWithin30Days = expiring30,
            ExpiringWithin60Days = expiring60,
            ExpiringWithin90Days = expiring90,
            ChecklistComplete = checklistComplete,
            ChecklistItemsTotal = checklistTotal,
            ChecklistItemsCompleted = checklistCompleted,
            ComplianceScore = complianceScore
        };
    }
}
