using SECURA.Domain.Enums;

namespace SECURA.Application.Claims.DTOs;

public sealed class ClaimReferenceDto
{
    public Guid Id { get; init; }
    public Guid AgencyId { get; init; }
    public string? AgencyName { get; init; }
    public Guid? ProducerId { get; init; }
    public string ExternalClaimNumber { get; init; } = string.Empty;
    public LobType Lob { get; init; }
    public ClaimStatus Status { get; init; }
    public DateTime LossDate { get; init; }
    public string? InsuredName { get; init; }
    public string? Description { get; init; }
    public decimal? ReserveAmount { get; init; }
    public string? AssignedAdjusterUserId { get; init; }
    public DateTime? ClosedDate { get; init; }
    public DateTime CreatedAt { get; init; }
}
