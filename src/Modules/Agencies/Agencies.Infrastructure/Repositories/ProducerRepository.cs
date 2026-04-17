using Microsoft.EntityFrameworkCore;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.Agencies.Domain.Producers;
using Secura.DistributionCrm.Agencies.Infrastructure.Persistence;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Agencies.Infrastructure.Repositories;

public sealed class ProducerRepository
    : Repository<Producer, Guid, AgenciesDbContext>, IProducerRepository
{
    private readonly AgenciesDbContext _context;

    public ProducerRepository(AgenciesDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Producer> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        Guid? branchId = null, Guid? agencyId = null,
        LicenseStatus? licenseStatus = null, bool? activeOnly = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(p => p.Licenses)
            .Include(p => p.Branch)
            .AsQueryable();

        if (branchId.HasValue)
            query = query.Where(p => p.BranchId == branchId.Value);
        if (agencyId.HasValue)
            query = query.Where(p => p.Branch != null && p.Branch.AgencyId == agencyId.Value);
        if (licenseStatus.HasValue)
            query = query.Where(p => p.LicenseStatus == licenseStatus.Value);
        if (activeOnly == true)
            query = query.Where(p => p.ActiveFlag);
        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p =>
                p.FirstName.Contains(searchTerm) ||
                p.LastName.Contains(searchTerm) ||
                p.Npn.Value.Contains(searchTerm));

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<Producer?> GetWithLicensesAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(p => p.Licenses)
            .Include(p => p.Branch)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<bool> NpnExistsAsync(
        string npn, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(p => p.Npn.Value == npn, cancellationToken);

    public async Task<IReadOnlyList<License>> GetExpiringLicensesAsync(
        int withinDays, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(withinDays);
        return await _context.Licenses
            .Where(l => l.Status == LicenseStatus.Active && l.ExpirationDate <= cutoff)
            .OrderBy(l => l.ExpirationDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<License>> GetLicensesByAgencyAsync(
        Guid agencyId, CancellationToken cancellationToken = default)
        => await _context.Licenses
            .Include(l => l.Producer)
                .ThenInclude(p => p!.Branch)
            .Where(l => l.Producer != null &&
                        l.Producer.Branch != null &&
                        l.Producer.Branch.AgencyId == agencyId)
            .ToListAsync(cancellationToken);
}
