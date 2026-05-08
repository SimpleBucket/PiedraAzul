using System.Net;
using System.Net.Mail;
using PiedraAzul.Notifications.Models;

namespace PiedraAzul.Notifications.Services;

public class EmailSender
{
    private readonly SmtpClient _smtp;
    private readonly string _fromAddress;
    private readonly string _fromName;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IConfiguration config, ILogger<EmailSender> logger)
    {
        _logger = logger;
        var section = config.GetSection("Email:Smtp");
        _fromAddress = section["FromAddress"] ?? "noreply@piedraazul.com";
        _fromName    = section["FromName"]    ?? "Piedra Azul";

        _smtp = new SmtpClient(section["Host"] ?? "smtp.gmail.com", int.Parse(section["Port"] ?? "587"))
        {
            Credentials = new NetworkCredential(section["Username"], section["Password"]),
            EnableSsl   = true,
            Timeout     = 10_000
        };
    }

    public async Task<bool> SendAsync(EmailRequest req)
    {
        try
        {
            using var msg = new MailMessage
            {
                From       = new MailAddress(_fromAddress, _fromName),
                Subject    = req.Subject,
                Body       = req.HtmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(req.To);
            await _smtp.SendMailAsync(msg);
            _logger.LogInformation("Email sent to {To}", req.To);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", req.To);
            return false;
        }
    }
}
