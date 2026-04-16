using MediatR;
using SECURA.Application.Common.Interfaces;
using SECURA.Application.Producers.DTOs;
using SECURA.Domain.Enums;

namespace SECURA.Application.Compliance.Queries;

public sealed record GetExpiringLicensesQuery(int WithinDays = 30)
    : IRequest<IReadOnlyList<LicenseDto>>;

public sealed class GetExpiringLicensesQueryHandler
    : IRequestHandler<GetExpiringLicensesQuery, IReadOnlyList<LicenseDto>>
{
    private readonly IProducerRepository _producers;

    public GetExpiringLicensesQueryHandler(IProducerRepository producers)
    {
        _producers = producers;
    }

    public async Task<IReadOnlyList<LicenseDto>> Handle(
        GetExpiringLicensesQuery request, CancellationToken cancellationToken)
    {
        var licenses = await _producers.GetExpiringLicensesAsync(request.WithinDays, cancellationToken);

        return licenses.Select(l => new LicenseDto
        {
            Id = l.Id,
            State = l.State,
            Lob = l.Lob,
            Status = l.Status,
            ExpirationDate = l.ExpirationDate,
            LicenseNumber = l.LicenseNumber,
            IsExpired = l.IsExpired
        }).ToList();
    }
}
