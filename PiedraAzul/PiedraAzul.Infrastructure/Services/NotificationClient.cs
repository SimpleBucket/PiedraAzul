using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PiedraAzul.Application.Common.Interfaces;

namespace PiedraAzul.Infrastructure.Services;

public class NotificationClient : INotificationClient
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly string _baseUrl;
    private readonly ILogger<NotificationClient> _logger;

    public NotificationClient(IHttpClientFactory httpFactory, IConfiguration config, ILogger<NotificationClient> logger)
    {
        _httpFactory = httpFactory;
        _logger      = logger;
        _baseUrl     = config["Notifications:BaseUrl"] ?? "https://localhost:7200";
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        try
        {
            var client = _httpFactory.CreateClient("Notifications");
            var payload = JsonSerializer.Serialize(new { to, subject, htmlBody });
            await client.PostAsync(
                $"{_baseUrl}/notifications/email",
                new StringContent(payload, Encoding.UTF8, "application/json"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reach notification service for email to {To}", to);
        }
    }

    public async Task SendWhatsAppAsync(string phoneE164, string message)
    {
        try
        {
            var client = _httpFactory.CreateClient("Notifications");
            var payload = JsonSerializer.Serialize(new { phoneE164, message });
            await client.PostAsync(
                $"{_baseUrl}/notifications/whatsapp",
                new StringContent(payload, Encoding.UTF8, "application/json"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reach notification service for WhatsApp to {Phone}", phoneE164);
        }
    }
}
