namespace PiedraAzul.Application.Common.Interfaces;

public interface IAuditClient
{
    Task LogAsync(string action, string entityType, string? entityId, string? userId, string? detail = null);
}
