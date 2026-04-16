using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Application.Common.Interfaces;

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
