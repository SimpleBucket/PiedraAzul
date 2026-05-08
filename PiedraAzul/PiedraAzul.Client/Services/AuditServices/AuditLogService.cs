using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PiedraAzul.Client.Services.AuditServices;

public class AuditLogEntry
{
    public int Id { get; set; }
    public string Action { get; set; } = default!;
    public string EntityType { get; set; } = default!;
    public string? EntityId { get; set; }
    public string? UserId { get; set; }
    public string? Detail { get; set; }
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }
}

public class AuditLogService(HttpClient http)
{
    public async Task<List<AuditLogEntry>> GetLogsAsync(
        string? userId = null,
        string? entityType = null,
        int page = 1,
        int pageSize = 50)
    {
        var qs = $"/api/audit?page={page}&pageSize={pageSize}";
        if (userId is not null) qs += $"&userId={Uri.EscapeDataString(userId)}";
        if (entityType is not null) qs += $"&entityType={Uri.EscapeDataString(entityType)}";

        try
        {
            return await http.GetFromJsonAsync<List<AuditLogEntry>>(qs) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
