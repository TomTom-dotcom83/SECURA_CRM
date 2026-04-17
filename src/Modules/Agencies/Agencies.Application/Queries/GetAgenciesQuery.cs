using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Application.Models;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.Agencies.Application.DTOs;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Application.Queries;

public sealed record GetAgenciesQuery(
    int Page = 1,
    int PageSize = 25,
    AgencyStatus? Status = null,
    AgencyTier? Tier = null,
    string? State = null,
    string? SearchTerm = null) : IRequest<PagedResult<AgencySummaryDto>>;

public sealed class GetAgenciesQueryHandler
    : IRequestHandler<GetAgenciesQuery, PagedResult<AgencySummaryDto>>
{
    private readonly IAgencyRepository _agencies;

    public GetAgenciesQueryHandler(IAgencyRepository agencies)
    {
        _agencies = agencies;
    }

    public async Task<PagedResult<AgencySummaryDto>> Handle(
        GetAgenciesQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _agencies.GetPagedAsync(
            request.Page, request.PageSize,
            request.Status, request.Tier, request.State, request.SearchTerm,
            cancellationToken);

        var dtos = items.Select(a => new AgencySummaryDto
        {
            Id = a.Id,
            Name = a.Name,
            Status = a.Status,
            Tier = a.Tier,
            PrimaryState = a.PrimaryState,
            ProducerCount = a.Branches.SelectMany(b => b.Producers).Count(),
            CreatedAt = a.CreatedAt
        }).ToList();

        return PagedResult<AgencySummaryDto>.Create(dtos, total, request.Page, request.PageSize);
    }
}
