using MediatR;
using Secura.DistributionCrm.Agencies.Application.Abstractions;
using Secura.DistributionCrm.SharedKernel.Enums;

namespace Secura.DistributionCrm.Reporting.Application.Queries;

public sealed record GetOnboardingCycleTimeQuery : IRequest<OnboardingCycleTimeDto>;

public sealed record OnboardingCycleTimeDto(
    double AverageDays,
    double MedianDays,
    int SampleSize,
    DateTime GeneratedAt);

public sealed class GetOnboardingCycleTimeQueryHandler
    : IRequestHandler<GetOnboardingCycleTimeQuery, OnboardingCycleTimeDto>
{
    private readonly IAgencyRepository _agencies;

    public GetOnboardingCycleTimeQueryHandler(IAgencyRepository agencies)
    {
        _agencies = agencies;
    }

    public async Task<OnboardingCycleTimeDto> Handle(
        GetOnboardingCycleTimeQuery request, CancellationToken cancellationToken)
    {
        var (agencies, _) = await _agencies.GetPagedAsync(
            1, int.MaxValue, status: AgencyStatus.Active,
            cancellationToken: cancellationToken);

        var cycleTimes = agencies
            .Where(a => a.ModifiedAt.HasValue && a.ModifiedAt.Value > a.CreatedAt)
            .Select(a => (a.ModifiedAt!.Value - a.CreatedAt).TotalDays)
            .OrderBy(d => d)
            .ToList();

        if (cycleTimes.Count == 0)
            return new OnboardingCycleTimeDto(0, 0, 0, DateTime.UtcNow);

        var avg = Math.Round(cycleTimes.Average(), 1);
        var median = cycleTimes.Count % 2 == 0
            ? Math.Round((cycleTimes[cycleTimes.Count / 2 - 1] + cycleTimes[cycleTimes.Count / 2]) / 2, 1)
            : Math.Round(cycleTimes[cycleTimes.Count / 2], 1);

        return new OnboardingCycleTimeDto(avg, median, cycleTimes.Count, DateTime.UtcNow);
    }
}
