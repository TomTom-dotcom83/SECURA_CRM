using MediatR;
using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.Agencies.Application.DTOs;

namespace Secura.DistributionCrm.Agencies.Application.Queries;

public sealed record GetAgencyByIdQuery(Guid Id) : IRequest<AgencyDto>;

public sealed class GetAgencyByIdQueryHandler : IRequestHandler<GetAgencyByIdQuery, AgencyDto>
{
    private readonly IAgencyRepository _agencies;

    public GetAgencyByIdQueryHandler(IAgencyRepository agencies)
    {
        _agencies = agencies;
    }

    public async Task<AgencyDto> Handle(GetAgencyByIdQuery request, CancellationToken cancellationToken)
    {
        var agency = await _agencies.GetWithDetailsAsync(request.Id, cancellationToken)
            ?? throw new DomainException($"Agency {request.Id} not found.");

        return new AgencyDto
        {
            Id = agency.Id,
            Name = agency.Name,
            Status = agency.Status,
            Tier = agency.Tier,
            PrimaryState = agency.PrimaryState,
            Phone = agency.Phone,
            Email = agency.Email,
            Website = agency.Website,
            Notes = agency.Notes,
            CreatedAt = agency.CreatedAt,
            CreatedBy = agency.CreatedBy,
            ModifiedAt = agency.ModifiedAt,
            BranchCount = agency.Branches.Count,
            ProducerCount = agency.Branches.SelectMany(b => b.Producers).Count(),
            ActiveLicenseCount = agency.Branches
                .SelectMany(b => b.Producers)
                .SelectMany(p => p.Licenses)
                .Count(l => !l.IsExpired)
        };
    }
}
