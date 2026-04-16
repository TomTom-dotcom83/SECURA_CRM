using MediatR;
using SECURA.Application.Claims.DTOs;
using SECURA.Application.Common.Interfaces;
using SECURA.Application.Common.Models;
using SECURA.Domain.Enums;

namespace SECURA.Application.Claims.Queries;

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
            AgencyName = c.Agency?.Name,
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
