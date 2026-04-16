using MediatR;
using SECURA.Application.Compliance.Queries;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;
using SECURA.Infrastructure.Persistence;

namespace SECURA.Infrastructure.Tests;

[Trait("Category", "E2E")]
[Collection("E2E")]
public sealed class ComplianceAlertTests : IClassFixture<SecuraWebApplicationFactory>
{
    private readonly SecuraWebApplicationFactory _factory;

    public ComplianceAlertTests(SecuraWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetExpiringLicenses_Returns_Licenses_Expiring_Within_30_Days()
    {
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var db = scope.ServiceProvider.GetRequiredService<SecuraDbContext>();

        // Seed: agency, branch, producer with an expiring license
        var agency = Agency.Create("Compliance Test Agency", AgencyTier.Standard, "TX");
        var branch = Branch.Create(agency.Id, "Main Branch");
        var producer = Producer.Create(branch.Id, "9876543210", "Jane", "Smith");

        var expiringLicense = SECURA.Domain.Entities.License.Create(
            producer.Id, "TX", LobType.BOP,
            LicenseStatus.Active, DateTime.UtcNow.AddDays(15), "LIC-9999");

        db.Agencies.Add(agency);
        db.Branches.Add(branch);
        db.Producers.Add(producer);
        db.Licenses.Add(expiringLicense);
        await db.SaveChangesAsync();

        // Query expiring licenses
        var expiring = await mediator.Send(new GetExpiringLicensesQuery(30));

        expiring.Should().NotBeEmpty();
        expiring.Should().Contain(l => l.LicenseNumber == "LIC-9999");
    }
}
