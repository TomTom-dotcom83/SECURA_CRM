using SECURA.Domain.Common;
using SECURA.Domain.Enums;
using SECURA.Domain.Events;

namespace SECURA.Domain.Entities;

public sealed class License : Entity<Guid>, IAuditableEntity
{
    private License() { }

    private License(Guid id, Guid producerId, string state, LobType lob,
        LicenseStatus status, DateTime expirationDate) : base(id)
    {
        ProducerId = producerId;
        State = state.ToUpperInvariant();
        Lob = lob;
        Status = status;
        ExpirationDate = expirationDate;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = string.Empty;
    }

    public Guid ProducerId { get; private set; }
    public string State { get; private set; } = string.Empty;
    public LobType Lob { get; private set; }
    public LicenseStatus Status { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public string? LicenseNumber { get; private set; }
    public DateTime? IssuedDate { get; private set; }

    public bool IsExpired => Status == LicenseStatus.Expired
                             || (Status == LicenseStatus.Active && ExpirationDate < DateTime.UtcNow);

    public bool ExpiresWithinDays(int days) =>
        Status == LicenseStatus.Active && ExpirationDate <= DateTime.UtcNow.AddDays(days);

    public Producer? Producer { get; private set; }

    // IAuditableEntity
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedAt { get; set; }
    public string? ModifiedBy { get; set; }

    public static License Create(Guid producerId, string state, LobType lob,
        LicenseStatus status, DateTime expirationDate, string? licenseNumber = null)
    {
        if (string.IsNullOrWhiteSpace(state))
            throw new DomainException("License state is required.");

        return new License(Guid.NewGuid(), producerId, state, lob, status, expirationDate)
        {
            LicenseNumber = licenseNumber
        };
    }

    public void Renew(DateTime newExpirationDate, string userId)
    {
        ExpirationDate = newExpirationDate;
        Status = LicenseStatus.Active;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;
    }

    public void UpdateStatus(LicenseStatus newStatus, string userId)
    {
        Status = newStatus;
        ModifiedAt = DateTime.UtcNow;
        ModifiedBy = userId;
    }
}
