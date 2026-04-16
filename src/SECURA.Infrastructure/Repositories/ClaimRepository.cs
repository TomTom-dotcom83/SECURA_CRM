using Microsoft.EntityFrameworkCore;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;
using SECURA.Infrastructure.Persistence;

namespace SECURA.Infrastructure.Repositories;

public sealed class ClaimRepository : Repository<ClaimReference, Guid>, IClaimRepository
{
    public ClaimRepository(SecuraDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<ClaimReference> Items, int TotalCount)> GetByAgencyPagedAsync(
        Guid agencyId, int page, int pageSize,
        ClaimStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(c => c.Agency)
            .Where(c => c.AgencyId == agencyId);

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

    public async Task<bool> ExternalNumberExistsAsync(string externalClaimNumber,
        CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(c => c.ExternalClaimNumber == externalClaimNumber, cancellationToken);
}
