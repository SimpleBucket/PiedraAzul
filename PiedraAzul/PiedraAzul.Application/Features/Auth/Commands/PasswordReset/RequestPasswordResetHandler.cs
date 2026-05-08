using Mediator;
using PiedraAzul.Application.Common.Interfaces;

namespace PiedraAzul.Application.Features.Auth.Commands.PasswordReset;

public class RequestPasswordResetHandler : IRequestHandler<RequestPasswordResetCommand, bool>
{
    private readonly IIdentityService _identityService;
    private readonly IEmailService _emailService;
    private readonly IAuditClient _audit;

    public RequestPasswordResetHandler(
        IIdentityService identityService,
        IEmailService emailService,
        IAuditClient audit)
    {
        _identityService = identityService;
        _emailService    = emailService;
        _audit           = audit;
    }

    public async ValueTask<bool> Handle(RequestPasswordResetCommand request, CancellationToken ct)
    {
        var resetToken = await _identityService.GeneratePasswordResetTokenAsync(request.Email);
        if (string.IsNullOrEmpty(resetToken))
            return false;

        var resetLink = $"https://piedraazul.runasp.net/account/reset-password?email={Uri.EscapeDataString(request.Email)}&token={Uri.EscapeDataString(resetToken)}";

        var ok = await _emailService.SendPasswordResetEmailAsync(request.Email, request.Email, resetLink);

        if (ok)
            await _audit.LogAsync("RequestPasswordReset", "User", null, null, request.Email);

        return ok;
    }
}
