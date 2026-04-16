using SECURA.Domain.Common;
using SECURA.Domain.Enums;
using SECURA.Domain.Events;
using SECURA.Domain.ValueObjects;

namespace SECURA.Domain.Entities;

public sealed class Producer : AggregateRoot<Guid>, IAuditableEntity
{
    private readonly List<License> _licenses = [];

    private Producer() { }

    private Producer(Guid id, Guid branchId, NationalProducerNumber npn,
        string firstName, string lastName) : base(id)
    {
        BranchId = branchId;
        Npn = npn;
        FirstName = firstName;
        LastName = lastName;
        ActiveFlag = true;
        LicenseStatus = LicenseStatus.Pending;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = string.Empty;
    }

    public Guid BranchId { get; private set; }
    public NationalProducerNumber Npn { get; private set; } = null!;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Email { get; private set; }
    public string? Phone { get; private set; }
    public LicenseStatus LicenseStatus { get; private set; }
    public bool ActiveFlag { get; private set; }
    public DateTime? TerminationDate { get; private set; }

    public IReadOnlyList<License> Licenses => _licenses.AsReadOnly();

    public Branch? Branch { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    public static Producer Create(Guid branchId, string npn, string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new DomainException("First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Last name is required.");

        var producer = new Producer(Guid.NewGuid(), branchId,
            NationalProducerNumber.Create(npn), firstName.Trim(), lastName.Trim());

        producer.AddDomainEvent(new ProducerCreatedEvent(producer.Id, branchId));
        return producer;
    }

    public void Update(string firstName, string lastName, string? email, string? phone, string userId)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email;
        Phone = phone;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;
    }

    public void UpdateLicenseStatus(LicenseStatus status, string userId)
    {
        LicenseStatus = status;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;
    }

    public void Terminate(DateTime terminationDate, string userId)
    {
        if (!ActiveFlag)
            throw new DomainException("Producer is already inactive.");

        ActiveFlag = false;
        TerminationDate = terminationDate;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;
    }

    public void AddLicense(License license) => _licenses.Add(license);
}
