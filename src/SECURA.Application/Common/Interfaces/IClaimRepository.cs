using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Application.Common.Interfaces;

public interface IClaimRepository : IRepository<ClaimReference, Guid>
{
    Task<(IReadOnlyList<ClaimReference> Items, int TotalCount)> GetByAgencyPagedAsync(
        Guid agencyId, int page, int pageSize,
        ClaimStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExternalNumberExistsAsync(string externalClaimNumber,
        CancellationToken cancellationToken = default);
}
