using Secura.DistributionCrm.BuildingBlocks.Domain.Primitives;

namespace Secura.DistributionCrm.BuildingBlocks.Application.Abstractions;

public interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Delete(TEntity entity);
}
