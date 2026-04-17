using Microsoft.EntityFrameworkCore;
using Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence;
using Secura.DistributionCrm.SharedKernel.Enums;
using Secura.DistributionCrm.Submissions.Application.Abstractions;
using Secura.DistributionCrm.Submissions.Domain;
using Secura.DistributionCrm.Submissions.Infrastructure.Persistence;

namespace Secura.DistributionCrm.Submissions.Infrastructure.Repositories;

public sealed class SubmissionRepository
    : Repository<Submission, Guid, SubmissionsDbContext>, ISubmissionRepository
{
    public SubmissionRepository(SubmissionsDbContext context) : base(context) { }

    public async Task<(IReadOnlyList<Submission> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        SubmissionStatus? status = null, LobType? lob = null,
        string? state = null, Guid? agencyId = null, bool? isOverdue = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

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

        if (isOverdue.HasValue)
            items = items.Where(s => s.IsOverdue == isOverdue.Value).ToList();

        return (items, total);
    }

    public async Task<Submission?> GetWithNotesAsync(
        Guid id, CancellationToken cancellationToken = default)
        => await DbSet
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

        static int SlaHoursFor(LobType lob) => lob switch
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
            s.ReceivedDate.AddHours(SlaHoursFor(s.Lob)) >= now);

        return (openSubmissions.Count, onTime);
    }
}
