using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Application.Abstractions;

public record NiprLicenseResult(
    bool IsValid,
    LicenseStatus Status,
    DateTime? ExpirationDate,
    string? LicenseNumber,
    string? ErrorMessage);

public interface INiprService
{
    Task<NiprLicenseResult> ValidateLicenseAsync(
        string npn, string state, LobType lob,
        CancellationToken cancellationToken = default);

    Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
}
