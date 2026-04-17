using Microsoft.EntityFrameworkCore;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.Agencies.Domain.Onboarding;
using Secura.DistributionCrm.Agencies.Infrastructure.Persistence;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence;

namespace Secura.DistributionCrm.Agencies.Infrastructure.Repositories;

public sealed class ChecklistRepository
    : Repository<OnboardingChecklist, Guid, AgenciesDbContext>, IChecklistRepository
{
    public ChecklistRepository(AgenciesDbContext context) : base(context) { }

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
