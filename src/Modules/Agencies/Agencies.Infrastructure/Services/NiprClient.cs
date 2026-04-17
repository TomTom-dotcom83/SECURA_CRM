using Microsoft.Extensions.Logging;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Infrastructure.Services;

/// <summary>
/// Stub implementation of INiprService. In production, replace with real NIPR/Sircon HTTP client
/// with Polly retry and circuit breaker policies.
/// </summary>
public sealed class NiprClient : INiprService
{
    private readonly ILogger<NiprClient> _logger;

    public NiprClient(ILogger<NiprClient> logger)
    {
        _logger = logger;
    }

    public Task<NiprLicenseResult> ValidateLicenseAsync(
        string npn, string state, LobType lob, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "NIPR stub: ValidateLicense NPN={Npn} State={State} LOB={Lob}", npn, state, lob);

        var result = new NiprLicenseResult(
            IsValid: true,
            Status: LicenseStatus.Active,
            ExpirationDate: DateTime.UtcNow.AddYears(1),
            LicenseNumber: $"STUB-{npn}-{state}",
            ErrorMessage: null);

        return Task.FromResult(result);
    }

    public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(true);
}
