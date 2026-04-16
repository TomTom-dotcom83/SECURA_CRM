using MediatR;
using SECURA.Application.Agencies.DTOs;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Common;

namespace SECURA.Application.Agencies.Queries;

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
