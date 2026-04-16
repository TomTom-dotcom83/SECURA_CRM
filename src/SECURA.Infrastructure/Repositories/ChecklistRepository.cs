using Microsoft.EntityFrameworkCore;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Entities;
using SECURA.Infrastructure.Persistence;

namespace SECURA.Infrastructure.Repositories;

public sealed class ChecklistRepository : Repository<OnboardingChecklist, Guid>, IChecklistRepository
{
    public ChecklistRepository(SecuraDbContext context) : base(context) { }

    public async Task<OnboardingChecklist?> GetByAgencyIdAsync(
        Guid agencyId, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.AgencyId == agencyId, cancellationToken);

    public async Task<OnboardingChecklist?> GetWithItemsAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<OnboardingChecklist>> GetIncompleteAsync(
        CancellationToken cancellationToken = default)
        => await DbSet
            .Include(c => c.Items)
            .Where(c => c.Items.Any(i => i.IsRequired && !i.IsCompleted))
            .ToListAsync(cancellationToken);
}
