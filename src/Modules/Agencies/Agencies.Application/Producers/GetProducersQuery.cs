using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Application.Models;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Application.Producers;

public sealed record GetProducersQuery(
    int Page = 1,
    int PageSize = 25,
    Guid? AgencyId = null,
    LicenseStatus? LicenseStatus = null,
    bool? ActiveOnly = null,
    string? SearchTerm = null) : IRequest<PagedResult<ProducerSummaryDto>>;

public sealed class GetProducersQueryHandler(IProducerRepository producers)
    : IRequestHandler<GetProducersQuery, PagedResult<ProducerSummaryDto>>
{
    public async Task<PagedResult<ProducerSummaryDto>> Handle(
        GetProducersQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await producers.GetPagedAsync(
            request.Page, request.PageSize,
            agencyId: request.AgencyId,
            licenseStatus: request.LicenseStatus,
            activeOnly: request.ActiveOnly,
            searchTerm: request.SearchTerm,
            cancellationToken: cancellationToken);

        var dtos = items.Select(p => new ProducerSummaryDto
        {
            Id               = p.Id,
            Npn              = p.Npn.Value,
            FullName         = p.FullName,
            LicenseStatus    = p.LicenseStatus,
            ActiveFlag       = p.ActiveFlag,
            ActiveLicenseCount = p.Licenses.Count(l => l.Status == LicenseStatus.Active)
        }).ToList();

        return PagedResult<ProducerSummaryDto>.Create(dtos, total, request.Page, request.PageSize);
    }
}
