using Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;
using Secura.DistributionCrm.Agencies.Domain.Onboarding;

namespace Secura.DistributionCrm.Agencies.Application.Abstractions;

public interface IChecklistRepository : IRepository<OnboardingChecklist, Guid>
{
    Task<OnboardingChecklist?> GetByAgencyIdAsync(Guid agencyId, CancellationToken cancellationToken = default);
    Task<OnboardingChecklist?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OnboardingChecklist>> GetIncompleteAsync(CancellationToken cancellationToken = default);
}
