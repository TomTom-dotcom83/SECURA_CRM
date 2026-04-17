using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.Claims.Domain;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Claims.Application.Abstractions;

public interface IClaimRepository : IRepository<ClaimReference, Guid>
{
    Task<(IReadOnlyList<ClaimReference> Items, int TotalCount)> GetByAgencyPagedAsync(
        Guid agencyId, int page, int pageSize,
        ClaimStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<bool> ExternalNumberExistsAsync(string externalClaimNumber,
        CancellationToken cancellationToken = default);
}
