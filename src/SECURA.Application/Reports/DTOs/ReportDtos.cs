using SECURA.Domain.Enums;

namespace SECURA.Application.Reports.DTOs;

public sealed class AgencyScorecardDto
{
    public Guid AgencyId { get; init; }
    public string AgencyName { get; init; } = string.Empty;
    public AgencyTier Tier { get; init; }
    public int TotalSubmissions { get; init; }
    public int BoundSubmissions { get; init; }
    public int DeclinedSubmissions { get; init; }
    public decimal HitRatio => TotalSubmissions == 0
        ? 0 : (decimal)BoundSubmissions / TotalSubmissions * 100;
    public int ActiveProducers { get; init; }
    public int ExpiringLicenses { get; init; }
    public int OpenClaims { get; init; }
    public double AvgDaysToOnboard { get; init; }
    public DateTime GeneratedAt { get; init; }
}

public sealed class PipelineReportDto
{
    public int NewCount { get; init; }
    public int TriagedCount { get; init; }
    public int InReviewCount { get; init; }
    public int ReferredCount { get; init; }
    public int QuotedCount { get; init; }
    public int BoundCount { get; init; }
    public int DeclinedCount { get; init; }
    public int OverdueCount { get; init; }
    public decimal SlaAdherencePercent { get; init; }
    public DateTime GeneratedAt { get; init; }
}

public sealed class ComplianceSummaryDto
{
    public int TotalProducers { get; init; }
    public int ActiveLicenses { get; init; }
    public int ExpiredLicenses { get; init; }
    public int ExpiringIn30Days { get; init; }
    public int ExpiringIn60Days { get; init; }
    public int ExpiringIn90Days { get; init; }
    public DateTime GeneratedAt { get; init; }
}
