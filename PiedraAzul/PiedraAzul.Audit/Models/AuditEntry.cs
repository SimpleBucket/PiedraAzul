namespace PiedraAzul.Audit.Models;

public class AuditEntry
{
    public int Id { get; set; }
    public string Action { get; set; } = default!;
    public string EntityType { get; set; } = default!;
    public string? EntityId { get; set; }
    public string? UserId { get; set; }
    public string? Detail { get; set; }
    public DateTime Timestamp { get; set; }
}

public record AuditRequest(
    string Action,
    string EntityType,
    string? EntityId,
    string? UserId,
    string? Detail,
    DateTimeOffset Timestamp
);
