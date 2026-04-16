using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SECURA.Application.Compliance.Queries;

namespace SECURA.Infrastructure.BackgroundServices;

/// <summary>
/// Nightly compliance check: finds expiring/expired licenses and fires domain events.
/// Runs once per day at startup, then every 24 hours.
/// </summary>
public sealed class ComplianceCheckService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ComplianceCheckService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    public ComplianceCheckService(IServiceScopeFactory scopeFactory, ILogger<ComplianceCheckService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ComplianceCheckService started.");

        using var timer = new PeriodicTimer(Interval);
        do
        {
            await RunComplianceCheckAsync(stoppingToken);
        }
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task RunComplianceCheckAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Running nightly compliance check...");
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Check licenses expiring within 30, 60, 90 days
            foreach (var days in new[] { 30, 60, 90 })
            {
                var expiring = await mediator.Send(
                    new GetExpiringLicensesQuery(days), cancellationToken);
                _logger.LogInformation(
                    "ComplianceCheck: {Count} licenses expiring within {Days} days",
                    expiring.Count, days);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during compliance check.");
        }
    }
}
