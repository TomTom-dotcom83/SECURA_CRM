using Secura.DistributionCrm.Submissions.Domain;

namespace Secura.DistributionCrm.Administration.Application.AppetiteTags;

public interface IAppetiteTagRepository
{
    Task<IReadOnlyList<AppetiteTag>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<AppetiteTag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(AppetiteTag tag, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
