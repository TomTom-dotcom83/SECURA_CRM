using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Application.Models;
using Secura.DistributionCrm.Claims.Application.Abstractions;
using Secura.DistributionCrm.Claims.Application.DTOs;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Claims.Application.Queries;

public sealed record GetClaimsByAgencyQuery(
    Guid AgencyId,
    int Page = 1,
    int PageSize = 25,
    ClaimStatus? Status = null) : IRequest<PagedResult<ClaimReferenceDto>>;

public sealed class GetClaimsByAgencyQueryHandler
    : IRequestHandler<GetClaimsByAgencyQuery, PagedResult<ClaimReferenceDto>>
{
    private readonly IClaimRepository _claims;

    public GetClaimsByAgencyQueryHandler(IClaimRepository claims)
    {
        _claims = claims;
    }

    public async Task<PagedResult<ClaimReferenceDto>> Handle(
        GetClaimsByAgencyQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _claims.GetByAgencyPagedAsync(
            request.AgencyId, request.Page, request.PageSize,
            request.Status, cancellationToken);

        var dtos = items.Select(c => new ClaimReferenceDto
        {
            Id = c.Id,
            AgencyId = c.AgencyId,
            ProducerId = c.ProducerId,
            ExternalClaimNumber = c.ExternalClaimNumber,
            Lob = c.Lob,
            Status = c.Status,
            LossDate = c.LossDate,
            InsuredName = c.InsuredName,
            Description = c.Description,
            ReserveAmount = c.ReserveAmount,
            AssignedAdjusterUserId = c.AssignedAdjusterUserId,
            ClosedDate = c.ClosedDate,
            CreatedAt = c.CreatedAt
        }).ToList();

        return PagedResult<ClaimReferenceDto>.Create(dtos, total, request.Page, request.PageSize);
    }
}
