namespace PiedraAzul.Application.Common.Interfaces;

public interface INotificationClient
{
    Task SendEmailAsync(string to, string subject, string htmlBody);
    Task SendWhatsAppAsync(string phoneE164, string message);
}
