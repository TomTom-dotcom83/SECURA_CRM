using Microsoft.EntityFrameworkCore;
using SECURA.Application.Common.Interfaces;
using SECURA.Domain.Entities;
using SECURA.Domain.Enums;
using SECURA.Infrastructure.Persistence;

namespace SECURA.Infrastructure.Repositories;

public sealed class SubmissionRepository : Repository<Submission, Guid>, ISubmissionRepository
{
    public SubmissionRepository(SecuraDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<Submission> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        SubmissionStatus? status, LobType? lob, string? state,
        Guid? agencyId, bool? isOverdue,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(s => s.Agency)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);
        if (lob.HasValue)
            query = query.Where(s => s.Lob == lob.Value);
        if (!string.IsNullOrWhiteSpace(state))
            query = query.Where(s => s.State == state.ToUpperInvariant());
        if (agencyId.HasValue)
            query = query.Where(s => s.AgencyId == agencyId.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(s => s.ReceivedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // isOverdue filtering done in memory since SlaDeadline is computed
        if (isOverdue.HasValue)
            items = items.Where(s => s.IsOverdue == isOverdue.Value).ToList();

        return (items, total);
    }

    public async Task<Submission?> GetWithNotesAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(s => s.Agency)
            .Include(s => s.UWNotes)
            .Include(s => s.AppetiteTags)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyDictionary<SubmissionStatus, int>> GetStatusCountsAsync(
        CancellationToken cancellationToken = default)
    {
        var counts = await DbSet
            .GroupBy(s => s.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task<(int Total, int Bound, int Declined)> GetAgencySubmissionStatsAsync(
        Guid agencyId, CancellationToken cancellationToken = default)
    {
        var stats = await DbSet
            .Where(s => s.AgencyId == agencyId)
            .GroupBy(s => s.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var total = stats.Sum(x => x.Count);
        var bound = stats.Where(x => x.Status == SubmissionStatus.Bound).Sum(x => x.Count);
        var declined = stats.Where(x => x.Status == SubmissionStatus.Declined).Sum(x => x.Count);

        return (total, bound, declined);
    }

    public async Task<(int Total, int OnTime)> GetSlaStatsAsync(
        CancellationToken cancellationToken = default)
    {
        // Use List<T> so EF Core translates Contains to SQL IN (...) rather than
        // attempting the ReadOnlySpan overload that .NET 10 resolves on T[].
        var openStatuses = new List<SubmissionStatus>
        {
            SubmissionStatus.New, SubmissionStatus.Triaged,
            SubmissionStatus.InReview, SubmissionStatus.Referred,
            SubmissionStatus.Quoted
        };

        var openSubmissions = await DbSet
            .Where(s => openStatuses.Contains(s.Status))
            .Select(s => new { s.ReceivedDate, s.Lob })
            .ToListAsync(cancellationToken);

        if (openSubmissions.Count == 0)
            return (0, 0);

        // Reproduce SLA hours logic from domain (avoid in-memory entity instantiation)
        static int SlaHours(LobType lob) => lob switch
        {
            LobType.BOP => 48,
            LobType.CommercialAuto => 48,
            LobType.GeneralLiability => 72,
            LobType.CommercialProperty => 72,
            LobType.WorkersCompensation => 96,
            LobType.CommercialUmbrella => 120,
            LobType.PersonalUmbrella => 120,
            LobType.ProfessionalLiability => 120,
            LobType.CyberLiability => 48,
            _ => 72
        };

        var now = DateTime.UtcNow;
        var onTime = openSubmissions.Count(s =>
            s.ReceivedDate.AddHours(SlaHours(s.Lob)) >= now);

        return (openSubmissions.Count, onTime);
    }
}
