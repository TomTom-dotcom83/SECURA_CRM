namespace SECURA.Application.Compliance.DTOs;

public sealed class ComplianceStatusDto
{
    public Guid AgencyId { get; init; }
    public string AgencyName { get; init; } = string.Empty;
    public int TotalLicenses { get; init; }
    public int ActiveLicenses { get; init; }
    public int ExpiredLicenses { get; init; }
    public int ExpiringWithin30Days { get; init; }
    public int ExpiringWithin60Days { get; init; }
    public int ExpiringWithin90Days { get; init; }
    public bool ChecklistComplete { get; init; }
    public int ChecklistItemsTotal { get; init; }
    public int ChecklistItemsCompleted { get; init; }
    public double ComplianceScore { get; init; }
}

public sealed class ChecklistDto
{
    public Guid Id { get; init; }
    public Guid AgencyId { get; init; }
    public string TemplateName { get; init; } = string.Empty;
    public bool IsComplete { get; init; }
    public List<ChecklistItemDto> Items { get; init; } = [];
}

public sealed class ChecklistItemDto
{
    public Guid Id { get; init; }
    public string StepName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int Order { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? CompletedByUserId { get; init; }
    public bool IsRequired { get; init; }
}
