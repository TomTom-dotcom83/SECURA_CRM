using SECURA.Domain.Entities;

namespace SECURA.Application.Common.Interfaces;

public interface IChecklistRepository : IRepository<OnboardingChecklist, Guid>
{
    Task<OnboardingChecklist?> GetByAgencyIdAsync(Guid agencyId, CancellationToken cancellationToken = default);
    Task<OnboardingChecklist?> GetWithItemsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OnboardingChecklist>> GetIncompleteAsync(CancellationToken cancellationToken = default);
}
