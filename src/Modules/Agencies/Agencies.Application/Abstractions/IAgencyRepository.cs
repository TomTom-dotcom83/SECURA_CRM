using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.Agencies.Domain.Agencies;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Application.Abstractions;

public interface IAgencyRepository : IRepository<Agency, Guid>
{
    Task<(IReadOnlyList<Agency> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        AgencyStatus? status = null,
        AgencyTier? tier = null,
        string? state = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    Task<Agency?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
