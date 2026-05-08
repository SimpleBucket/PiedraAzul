namespace PiedraAzul.Notifications.Models;

public record EmailRequest(
    string To,
    string Subject,
    string HtmlBody
);

public record WhatsAppRequest(
    string PhoneE164,
    string Message
);
