using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Secura.DistributionCrm.Agencies.Infrastructure.Webhooks;

public sealed class WebhookDispatcher
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookDispatcher> _logger;
    private readonly string? _webhookUrl;

    public WebhookDispatcher(
        IHttpClientFactory httpClientFactory,
        ILogger<WebhookDispatcher> logger,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _webhookUrl = configuration["Webhooks:EndpointUrl"];
    }

    public async Task SendAsync(string eventType, object payload,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_webhookUrl))
            return;

        var envelope = new
        {
            EventType = eventType,
            OccurredOn = DateTime.UtcNow,
            Payload = payload
        };

        try
        {
            using var client = _httpClientFactory.CreateClient("webhook");
            var response = await client.PostAsJsonAsync(_webhookUrl, envelope, cancellationToken);
            response.EnsureSuccessStatusCode();
            _logger.LogInformation("Webhook sent: {EventType}", eventType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Webhook delivery failed for {EventType}", eventType);
        }
    }
}
