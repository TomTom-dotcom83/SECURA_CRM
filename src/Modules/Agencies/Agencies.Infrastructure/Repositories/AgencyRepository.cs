using Microsoft.EntityFrameworkCore;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.Agencies.Domain.Agencies;
using Secura.DistributionCrm.Agencies.Infrastructure.Persistence;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Infrastructure.Repositories;

public sealed class AgencyRepository
    : Repository<Agency, Guid, AgenciesDbContext>, IAgencyRepository
{
    public AgencyRepository(AgenciesDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<Agency> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        AgencyStatus? status = null, AgencyTier? tier = null,
        string? state = null, string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(a => a.Branches)
                .ThenInclude(b => b.Producers)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);
        if (tier.HasValue)
            query = query.Where(a => a.Tier == tier.Value);
        if (!string.IsNullOrWhiteSpace(state))
            query = query.Where(a => a.PrimaryState == state.ToUpperInvariant());
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(a => a.Name.Contains(searchTerm));

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Agency?> GetWithDetailsAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(a => a.Branches)
                .ThenInclude(b => b.Producers)
                    .ThenInclude(p => p.Licenses)
            .Include(a => a.Contracts)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<bool> ExistsAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(a => a.Id == id, cancellationToken);
}
