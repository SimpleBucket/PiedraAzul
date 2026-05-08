using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using PiedraAzul.Notifications.Models;

namespace PiedraAzul.Notifications.Services;

public class WhatsAppSender
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<WhatsAppSender> _logger;

    public WhatsAppSender(IHttpClientFactory httpFactory, IConfiguration config, ILogger<WhatsAppSender> logger)
    {
        _httpFactory = httpFactory;
        _config      = config;
        _logger      = logger;
    }

    public async Task<bool> SendAsync(WhatsAppRequest req)
    {
        try
        {
            var accessToken   = _config["WhatsApp:AccessToken"]   ?? throw new InvalidOperationException("WhatsApp:AccessToken missing");
            var phoneNumberId = _config["WhatsApp:PhoneNumberId"] ?? throw new InvalidOperationException("WhatsApp:PhoneNumberId missing");
            var apiVersion    = _config["WhatsApp:ApiVersion"]    ?? "v25.0";

            var payload = new
            {
                messaging_product = "whatsapp",
                to   = req.PhoneE164,
                type = "text",
                text = new { body = req.Message }
            };

            var client = _httpFactory.CreateClient("WhatsApp");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.PostAsync(
                $"https://graph.facebook.com/{apiVersion}/{phoneNumberId}/messages",
                new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

            _logger.LogInformation("WhatsApp sent to {Phone}, status {Status}", req.PhoneE164, response.StatusCode);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send WhatsApp to {Phone}", req.PhoneE164);
            return false;
        }
    }
}
