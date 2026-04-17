using MediatR;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Application.Compliance;

public sealed record GetExpiringLicensesQuery(int WithinDays = 30)
    : IRequest<IReadOnlyList<LicenseExpiryDto>>;

public sealed class LicenseExpiryDto
{
    public Guid Id { get; init; }
    public string State { get; init; } = string.Empty;
    public LobType Lob { get; init; }
    public LicenseStatus Status { get; init; }
    public DateTime ExpirationDate { get; init; }
    public string? LicenseNumber { get; init; }
    public bool IsExpired { get; init; }
}

public sealed class GetExpiringLicensesQueryHandler
    : IRequestHandler<GetExpiringLicensesQuery, IReadOnlyList<LicenseExpiryDto>>
{
    private readonly IProducerRepository _producers;

    public GetExpiringLicensesQueryHandler(IProducerRepository producers)
    {
        _producers = producers;
    }

    public async Task<IReadOnlyList<LicenseExpiryDto>> Handle(
        GetExpiringLicensesQuery request, CancellationToken cancellationToken)
    {
        var licenses = await _producers.GetExpiringLicensesAsync(request.WithinDays, cancellationToken);

        return licenses.Select(l => new LicenseExpiryDto
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
