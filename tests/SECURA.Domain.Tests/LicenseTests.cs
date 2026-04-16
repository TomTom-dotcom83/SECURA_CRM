using SECURA.Domain.Entities;
using SECURA.Domain.Enums;
using DomainLicense = SECURA.Domain.Entities.License;

namespace SECURA.Domain.Tests;

public sealed class LicenseTests
{
    [Fact]
    public void License_IsExpired_When_ExpirationDate_Past()
    {
        var license = DomainLicense.Create(
            Guid.NewGuid(), "TX", LobType.CommercialAuto,
            LicenseStatus.Active, DateTime.UtcNow.AddDays(-1));

        license.IsExpired.Should().BeTrue();
    }

    [Fact]
    public void License_Not_Expired_When_ExpirationDate_Future()
    {
        var license = DomainLicense.Create(
            Guid.NewGuid(), "TX", LobType.CommercialAuto,
            LicenseStatus.Active, DateTime.UtcNow.AddDays(30));

        license.IsExpired.Should().BeFalse();
    }

    [Fact]
    public void License_ExpiresWithinDays_Returns_True_When_Expiring_Soon()
    {
        var license = DomainLicense.Create(
            Guid.NewGuid(), "CA", LobType.GeneralLiability,
            LicenseStatus.Active, DateTime.UtcNow.AddDays(15));

        license.ExpiresWithinDays(30).Should().BeTrue();
        license.ExpiresWithinDays(10).Should().BeFalse();
    }

    [Fact]
    public void License_Renew_Extends_ExpirationDate()
    {
        var license = DomainLicense.Create(
            Guid.NewGuid(), "TX", LobType.CommercialAuto,
            LicenseStatus.Active, DateTime.UtcNow.AddDays(10));

        var newExpiry = DateTime.UtcNow.AddYears(1);
        license.Renew(newExpiry, "admin");

        license.ExpirationDate.Should().BeCloseTo(newExpiry, TimeSpan.FromSeconds(1));
        license.Status.Should().Be(LicenseStatus.Active);
    }
}
