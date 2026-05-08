using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PiedraAzul.Application.Common.Interfaces;

namespace PiedraAzul.Infrastructure.Services;

public class AuditClient : IAuditClient
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly string _baseUrl;
    private readonly ILogger<AuditClient> _logger;

    public AuditClient(IHttpClientFactory httpFactory, IConfiguration config, ILogger<AuditClient> logger)
    {
        _httpFactory = httpFactory;
        _logger      = logger;
        _baseUrl     = config["Audit:BaseUrl"] ?? "https://localhost:49412";
    }

    public async Task LogAsync(string action, string entityType, string? entityId, string? userId, string? detail = null)
    {
        try
        {
            var payload = JsonSerializer.Serialize(new
            {
                action,
                entityType,
                entityId,
                userId,
                detail,
                timestamp = DateTimeOffset.UtcNow
            });

            var client = _httpFactory.CreateClient("Audit");
            await client.PostAsync(
                $"{_baseUrl}/audit",
                new StringContent(payload, Encoding.UTF8, "application/json"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reach audit service for action {Action}", action);
        }
    }
}
