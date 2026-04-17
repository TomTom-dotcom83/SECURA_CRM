using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Application.DTOs;

public sealed class AgencyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public AgencyStatus Status { get; init; }
    public string StatusDisplay => Status.ToString();
    public AgencyTier Tier { get; init; }
    public string TierDisplay => Tier.ToString();
    public string PrimaryState { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? Website { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime? ModifiedAt { get; init; }
    public int BranchCount { get; init; }
    public int ProducerCount { get; init; }
    public int ActiveLicenseCount { get; init; }
}

public sealed class AgencySummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public AgencyStatus Status { get; init; }
    public AgencyTier Tier { get; init; }
    public string PrimaryState { get; init; } = string.Empty;
    public int ProducerCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
