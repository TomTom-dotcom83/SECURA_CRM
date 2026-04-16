using SECURA.Domain.Entities;
using SECURA.Domain.Enums;

namespace SECURA.Application.Common.Interfaces;

public interface ISubmissionRepository : IRepository<Submission, Guid>
{
    Task<(IReadOnlyList<Submission> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        SubmissionStatus? status = null,
        LobType? lob = null,
        string? state = null,
        Guid? agencyId = null,
        bool? isOverdue = null,
        CancellationToken cancellationToken = default);

    Task<Submission?> GetWithNotesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<SubmissionStatus, int>> GetStatusCountsAsync(
        CancellationToken cancellationToken = default);

    Task<(int Total, int OnTime)> GetSlaStatsAsync(
        CancellationToken cancellationToken = default);

    Task<(int Total, int Bound, int Declined)> GetAgencySubmissionStatsAsync(
        Guid agencyId, CancellationToken cancellationToken = default);
}
