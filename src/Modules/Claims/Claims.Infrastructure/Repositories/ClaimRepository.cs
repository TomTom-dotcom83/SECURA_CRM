using Microsoft.EntityFrameworkCore;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence;
using Secura.DistributionCrm.Claims.Application.Abstractions;
using Secura.DistributionCrm.Claims.Domain;
using Secura.DistributionCrm.Claims.Infrastructure.Persistence;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Claims.Infrastructure.Repositories;

public sealed class ClaimRepository
    : Repository<ClaimReference, Guid, ClaimsDbContext>, IClaimRepository
{
    public ClaimRepository(ClaimsDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<ClaimReference> Items, int TotalCount)> GetByAgencyPagedAsync(
        Guid agencyId, int page, int pageSize,
        ClaimStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(c => c.AgencyId == agencyId);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(c => c.LossDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<bool> ExternalNumberExistsAsync(
        string externalClaimNumber, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(
            c => c.ExternalClaimNumber == externalClaimNumber, cancellationToken);
}
