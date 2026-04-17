using Microsoft.EntityFrameworkCore;
using Secura.DistributionCrm.Administration.Application.AppetiteTags;
using Secura.DistributionCrm.Administration.Infrastructure.Persistence;
using Secura.DistributionCrm.Submissions.Domain;

namespace Secura.DistributionCrm.Administration.Infrastructure.Repositories;

public sealed class AppetiteTagRepository : IAppetiteTagRepository
{
    private readonly AdministrationDbContext _context;

    public AppetiteTagRepository(AdministrationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AppetiteTag>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => await _context.AppetiteTags
            .OrderBy(t => t.Label)
            .ToListAsync(cancellationToken);

    public async Task<AppetiteTag?> GetByIdAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await _context.AppetiteTags.FindAsync([id], cancellationToken);

    public async Task AddAsync(AppetiteTag tag, CancellationToken cancellationToken = default)
        => await _context.AppetiteTags.AddAsync(tag, cancellationToken);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
