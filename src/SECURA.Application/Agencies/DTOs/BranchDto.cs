namespace SECURA.Application.Agencies.DTOs;

public sealed class BranchDto
{
    public Guid Id { get; init; }
    public Guid AgencyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? State { get; init; }
    public bool IsActive { get; init; }
    public int ProducerCount { get; init; }
}
