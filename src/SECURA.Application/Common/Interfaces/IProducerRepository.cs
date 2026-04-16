using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Application.Common.Interfaces;

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
