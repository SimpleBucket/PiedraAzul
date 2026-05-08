using Mediator;
using PiedraAzul.Application.Common.Interfaces;

namespace PiedraAzul.Application.Features.Auth.Commands.MFA;

public class ConfirmTOTPSetupHandler : IRequestHandler<ConfirmTOTPSetupCommand, bool>
{
    private readonly IMFAService _mfaService;
    private readonly IAuditClient _audit;

    public ConfirmTOTPSetupHandler(IMFAService mfaService, IAuditClient audit)
    {
        _mfaService = mfaService;
        _audit      = audit;
    }

    public async ValueTask<bool> Handle(ConfirmTOTPSetupCommand request, CancellationToken ct)
    {
        var ok = await _mfaService.ConfirmTOTPSetupAsync(request.UserId, request.TOTP);

        if (ok)
            await _audit.LogAsync("MFASetup", "User", request.UserId, request.UserId, "TOTP");

        return ok;
    }
}
