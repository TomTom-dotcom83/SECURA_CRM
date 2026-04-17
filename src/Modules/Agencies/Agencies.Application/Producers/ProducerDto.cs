using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Application.Producers;

public sealed class ProducerDto
{
    public Guid Id { get; init; }
    public Guid BranchId { get; init; }
    public string Npn { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public LicenseStatus LicenseStatus { get; init; }
    public bool ActiveFlag { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<LicenseDto> Licenses { get; init; } = [];
}

public sealed class ProducerSummaryDto
{
    public Guid Id { get; init; }
    public string Npn { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public LicenseStatus LicenseStatus { get; init; }
    public bool ActiveFlag { get; init; }
    public int ActiveLicenseCount { get; init; }
}

public sealed class LicenseDto
{
    public Guid Id { get; init; }
    public string State { get; init; } = string.Empty;
    public LobType Lob { get; init; }
    public LicenseStatus Status { get; init; }
    public DateTime ExpirationDate { get; init; }
    public string? LicenseNumber { get; init; }
    public bool IsExpired { get; init; }
}
