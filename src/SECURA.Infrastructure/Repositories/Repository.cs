using Microsoft.EntityFrameworkCore;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Common;
using SECURA.Infrastructure.Persistence;

namespace SECURA.Infrastructure.Repositories;

public class Repository<TEntity, TId> : IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    protected readonly SecuraDbContext Context;
    protected readonly DbSet<TEntity> DbSet;

    public Repository(SecuraDbContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        => await DbSet.FindAsync([id], cancellationToken);

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
        => await DbSet.AddAsync(entity, cancellationToken);

    public virtual void Update(TEntity entity)
        => DbSet.Update(entity);

    public virtual void Delete(TEntity entity)
        => DbSet.Remove(entity);
}
