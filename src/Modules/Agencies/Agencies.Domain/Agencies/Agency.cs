using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Primitives;
using Secura.DistributionCrm.Agencies.Domain.Events;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Domain.Agencies;

public sealed class Agency : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<Branch> _branches = [];
    private readonly List<Contract> _contracts = [];

    // State machine — valid forward transitions
    private static readonly Dictionary<AgencyStatus, AgencyStatus[]> AllowedTransitions = new()
    {
        [AgencyStatus.Intake]       = [AgencyStatus.Validation, AgencyStatus.Rejected],
        [AgencyStatus.Validation]   = [AgencyStatus.Contracting, AgencyStatus.Rejected],
        [AgencyStatus.Contracting]  = [AgencyStatus.Appointment, AgencyStatus.Rejected],
        [AgencyStatus.Appointment]  = [AgencyStatus.Training, AgencyStatus.Rejected],
        [AgencyStatus.Training]     = [AgencyStatus.Active, AgencyStatus.Rejected],
        [AgencyStatus.Active]       = [AgencyStatus.Suspended, AgencyStatus.Terminated],
        [AgencyStatus.Suspended]    = [AgencyStatus.Active, AgencyStatus.Terminated],
        [AgencyStatus.Rejected]     = [],
        [AgencyStatus.Terminated]   = []
    };

    private Agency() { }

    private Agency(Guid id, string name, AgencyTier tier, string primaryState) : base(id)
    {
        Name = name;
        Tier = tier;
        PrimaryState = primaryState.ToUpperInvariant();
        Status = AgencyStatus.Intake;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = string.Empty;
    }

    public string Name { get; private set; } = string.Empty;
    public AgencyStatus Status { get; private set; }
    public AgencyTier Tier { get; private set; }
    public string PrimaryState { get; private set; } = string.Empty;
    public string? TaxId { get; private set; }
    public string? FeinNumber { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public Address? PhysicalAddress { get; private set; }
    public string? Website { get; private set; }
    public string? Notes { get; private set; }

    public IReadOnlyList<Branch> Branches => _branches.AsReadOnly();
    public IReadOnlyList<Contract> Contracts => _contracts.AsReadOnly();

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    public static Agency Create(string name, AgencyTier tier, string primaryState)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Agency name is required.");

        if (string.IsNullOrWhiteSpace(primaryState))
            throw new DomainException("Primary state is required.");

        var agency = new Agency(Guid.NewGuid(), name.Trim(), tier, primaryState);
        agency.AddDomainEvent(new AgencyStatusChangedEvent(agency.Id, null, AgencyStatus.Intake));
        return agency;
    }

    public bool CanTransitionTo(AgencyStatus targetStatus)
    {
        return AllowedTransitions.TryGetValue(Status, out var allowed)
               && allowed.Contains(targetStatus);
    }

    public void Transition(AgencyStatus targetStatus, string userId)
    {
        if (!CanTransitionTo(targetStatus))
            throw new DomainException(
                $"Cannot transition agency from {Status} to {targetStatus}.");

        var previous = Status;
        Status = targetStatus;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;

        AddDomainEvent(new AgencyStatusChangedEvent(Id, previous, targetStatus));

        if (targetStatus == AgencyStatus.Active)
            AddDomainEvent(new AgencyActivatedEvent(Id, userId));
    }

    public void Update(string name, AgencyTier tier, string primaryState,
        string? phone, string? email, string? website, string? notes, string userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Agency name is required.");

        Name = name.Trim();
        Tier = tier;
        PrimaryState = primaryState.ToUpperInvariant();
        Phone = phone;
        Email = email;
        Website = website;
        Notes = notes;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;
    }

    public void SetAddress(Address address)
    {
        PhysicalAddress = address;
    }

    internal void AddBranch(Branch branch) => _branches.Add(branch);
    internal void AddContract(Contract contract) => _contracts.Add(contract);
}
