using Microsoft.AspNetCore.Mvc;
using PiedraAzul.Notifications.Models;
using PiedraAzul.Notifications.Services;

namespace PiedraAzul.Notifications.Controllers;

[ApiController]
[Route("notifications")]
public class NotificationsController : ControllerBase
{
    private readonly EmailSender _email;
    private readonly WhatsAppSender _whatsApp;

    public NotificationsController(EmailSender email, WhatsAppSender whatsApp)
    {
        _email    = email;
        _whatsApp = whatsApp;
    }

    [HttpPost("email")]
    public async Task<IActionResult> SendEmail([FromBody] EmailRequest req)
    {
        var ok = await _email.SendAsync(req);
        return ok ? Ok() : StatusCode(502);
    }

    [HttpPost("whatsapp")]
    public async Task<IActionResult> SendWhatsApp([FromBody] WhatsAppRequest req)
    {
        var ok = await _whatsApp.SendAsync(req);
        return ok ? Ok() : StatusCode(502);
    }
}
