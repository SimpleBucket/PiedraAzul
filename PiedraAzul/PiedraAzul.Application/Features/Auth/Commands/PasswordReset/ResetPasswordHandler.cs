using Mediator;
using PiedraAzul.Application.Common.Interfaces;

namespace PiedraAzul.Application.Features.Auth.Commands.PasswordReset;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IIdentityService _identityService;
    private readonly IAuditClient _audit;

    public ResetPasswordHandler(IIdentityService identityService, IAuditClient audit)
    {
        _identityService = identityService;
        _audit           = audit;
    }

    public async ValueTask<bool> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var ok = await _identityService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);

        if (ok)
            await _audit.LogAsync("ResetPassword", "User", null, null, request.Email);

        return ok;
    }
}
