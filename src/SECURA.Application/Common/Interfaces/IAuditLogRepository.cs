using SECURA.Domain.Entities;

namespace SECURA.Application.Common.Interfaces;

public interface IAuditLogRepository
{
    Task AddAsync(AuditLog entry, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLog>> GetForEntityAsync(
        string entityType, string entityId,
        CancellationToken cancellationToken = default);
}
