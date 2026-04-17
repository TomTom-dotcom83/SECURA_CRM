using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Secura.DistributionCrm.BuildingBlocks.Domain.Events;

namespace Secura.DistributionCrm.BuildingBlocks.Infrastructure.Persistence.Outbox;

/// <summary>
/// Polls the OutboxMessages table and publishes domain events via MediatR.
/// Runs on a 5-second interval. Marks messages as processed after successful dispatch.
/// </summary>
public sealed class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);
    private const int MaxRetries = 5;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OutboxProcessor started.");

        using var timer = new PeriodicTimer(Interval);
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessBatchAsync(stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BaseDbContext>();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        var pending = await db.OutboxMessages
            .Where(m => m.ProcessedAt == null && m.RetryCount < MaxRetries)
            .OrderBy(m => m.OccurredOn)
            .Take(20)
            .ToListAsync(cancellationToken);

        foreach (var message in pending)
        {
            try
            {
                var eventType = Type.GetType(message.Type);
                if (eventType == null)
                {
                    _logger.LogWarning("OutboxProcessor: unknown event type {Type}", message.Type);
                    message.MarkFailed($"Unknown type: {message.Type}");
                    continue;
                }

                var domainEvent = JsonSerializer.Deserialize(message.Payload, eventType) as IDomainEvent;
                if (domainEvent != null)
                    await mediator.Publish(domainEvent, cancellationToken);

                message.MarkProcessed();
                _logger.LogDebug("OutboxProcessor: processed {Type} ({Id})", message.Type, message.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OutboxProcessor: failed to process {Id}", message.Id);
                message.MarkFailed(ex.Message);
            }
        }

        if (pending.Count > 0)
            await db.SaveChangesAsync(cancellationToken);
    }
}
