using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.Agencies.Domain.Producers;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Application.Abstractions;

public interface IProducerRepository : IRepository<Producer, Guid>
{
    Task<(IReadOnlyList<Producer> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Guid? branchId = null,
        Guid? agencyId = null,
        LicenseStatus? licenseStatus = null,
        bool? activeOnly = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<Producer?> GetWithLicensesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> NpnExistsAsync(string npn, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<License>> GetExpiringLicensesAsync(
        int withinDays, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<License>> GetLicensesByAgencyAsync(
        Guid agencyId, CancellationToken cancellationToken = default);
}
