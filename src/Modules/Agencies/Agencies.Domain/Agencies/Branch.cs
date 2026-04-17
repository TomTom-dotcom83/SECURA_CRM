using Secura.DistributionCrm.BuildingBlocks.Domain.Exceptions;
using Secura.DistributionCrm.BuildingBlocks.Domain.Primitives;
using Secura.DistributionCrm.Agencies.Domain.Producers;

namespace Secura.DistributionCrm.Agencies.Domain.Agencies;

public sealed class Branch : Entity<Guid>, IAuditableEntity
{
    private readonly List<Producer> _producers = [];

    private Branch() { }

    private Branch(Guid id, Guid agencyId, string name) : base(id)
    {
        AgencyId = agencyId;
        Name = name;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = string.Empty;
    }

    public Guid AgencyId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? State { get; private set; }
    public bool IsActive { get; private set; } = true;

    public IReadOnlyList<Producer> Producers => _producers.AsReadOnly();

    public Agency? Agency { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    public static Branch Create(Guid agencyId, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Branch name is required.");

        return new Branch(Guid.NewGuid(), agencyId, name.Trim());
    }

    public void Update(string name, string? phone, string? email, string? state, string userId)
    {
        Name = name.Trim();
        Phone = phone;
        Email = email;
        State = state;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;
    }

    public void Deactivate(string userId)
    {
        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;
    }

    internal void AddProducer(Producer producer) => _producers.Add(producer);
}
