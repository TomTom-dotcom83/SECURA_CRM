using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Primitives;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Claims.Domain;

public sealed class ClaimReference : Entity<Guid>, IAuditableEntity
{
    private ClaimReference() { }

    private ClaimReference(Guid id, Guid agencyId, string externalClaimNumber,
        LobType lob, DateTime lossDate) : base(id)
    {
        AgencyId = agencyId;
        ExternalClaimNumber = externalClaimNumber;
        Lob = lob;
        LossDate = lossDate;
        Status = ClaimStatus.Open;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = string.Empty;
    }

    public Guid AgencyId { get; private set; }
    public Guid? ProducerId { get; private set; }
    public string ExternalClaimNumber { get; private set; } = string.Empty;
    public LobType Lob { get; private set; }
    public ClaimStatus Status { get; private set; }
    public DateTime LossDate { get; private set; }
    public string? InsuredName { get; private set; }
    public string? Description { get; private set; }
    public decimal? ReserveAmount { get; private set; }
    public string? AssignedAdjusterUserId { get; private set; }
    public DateTime? ClosedDate { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    public static ClaimReference Create(Guid agencyId, string externalClaimNumber,
        LobType lob, DateTime lossDate)
    {
        if (string.IsNullOrWhiteSpace(externalClaimNumber))
            throw new DomainException("External claim number is required.");

        return new ClaimReference(Guid.NewGuid(), agencyId, externalClaimNumber, lob, lossDate);
    }

    public void UpdateStatus(ClaimStatus status, string userId)
    {
        Status = status;
        if (status == ClaimStatus.Closed)
            ClosedDate = DateTime.UtcNow;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;
    }
}
