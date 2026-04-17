using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Primitives;

namespace Secura.DistributionCrm.Agencies.Domain.Agencies;

public sealed class Contract : Entity<Guid>, IAuditableEntity
{
    private Contract() { }

    private Contract(Guid id, Guid agencyId, DateTime effectiveDate,
        string commissionScheduleRef) : base(id)
    {
        AgencyId = agencyId;
        EffectiveDate = effectiveDate;
        CommissionScheduleRef = commissionScheduleRef;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = string.Empty;
    }

    public Guid AgencyId { get; private set; }
    public DateTime EffectiveDate { get; private set; }
    public DateTime? TerminationDate { get; private set; }
    public string CommissionScheduleRef { get; private set; } = string.Empty;
    public string? ContractNumber { get; private set; }
    public string? Notes { get; private set; }

    public bool IsActive => TerminationDate == null || TerminationDate > DateTime.UtcNow;

    public Agency? Agency { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    public static Contract Create(Guid agencyId, DateTime effectiveDate,
        string commissionScheduleRef, string? contractNumber = null)
    {
        if (string.IsNullOrWhiteSpace(commissionScheduleRef))
            throw new DomainException("Commission schedule reference is required.");

        return new Contract(Guid.NewGuid(), agencyId, effectiveDate, commissionScheduleRef)
        {
            ContractNumber = contractNumber
        };
    }

    public void Terminate(DateTime terminationDate, string userId)
    {
        if (TerminationDate.HasValue)
            throw new DomainException("Contract is already terminated.");

        TerminationDate = terminationDate;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;
    }
}
