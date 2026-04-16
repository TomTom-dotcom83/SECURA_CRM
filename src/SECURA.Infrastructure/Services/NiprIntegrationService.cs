using Microsoft.Extensions.Logging;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Enums;

namespace SECURA.Infrastructure.Services;

/// <summary>
/// Stub implementation of INiprService. In production, replace with real NIPR/Sircon HTTP client
/// with Polly retry and circuit breaker policies.
/// </summary>
public sealed class NiprIntegrationService : INiprService
{
    private readonly ILogger<NiprIntegrationService> _logger;

    public NiprIntegrationService(ILogger<NiprIntegrationService> logger)
    {
        _logger = logger;
    }

    public Task<NiprLicenseResult> ValidateLicenseAsync(
        string npn, string state, LobType lob, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "NIPR stub: ValidateLicense NPN={Npn} State={State} LOB={Lob}", npn, state, lob);

        // Stub: always returns active with expiry 1 year from now
        var result = new NiprLicenseResult(
            IsValid: true,
            Status: LicenseStatus.Active,
            ExpirationDate: DateTime.UtcNow.AddYears(1),
            LicenseNumber: $"STUB-{npn}-{state}",
            ErrorMessage: null);

        return Task.FromResult(result);
    }

    public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default)
    {
        // Stub: always healthy
        return Task.FromResult(true);
    }
}
