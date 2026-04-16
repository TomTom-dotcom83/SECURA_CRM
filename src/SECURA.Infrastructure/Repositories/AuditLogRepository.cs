using Microsoft.EntityFrameworkCore;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Entities;
using SECURA.Infrastructure.Persistence;

namespace SECURA.Infrastructure.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly SecuraDbContext _context;

    public AuditLogRepository(SecuraDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(AuditLog entry, CancellationToken cancellationToken = default)
        => await _context.AuditLogs.AddAsync(entry, cancellationToken);

    public async Task<IReadOnlyList<AuditLog>> GetForEntityAsync(
        string entityType, string entityId, CancellationToken cancellationToken = default)
        => await _context.AuditLogs
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.Timestamp)
            .ToListAsync(cancellationToken);
}
