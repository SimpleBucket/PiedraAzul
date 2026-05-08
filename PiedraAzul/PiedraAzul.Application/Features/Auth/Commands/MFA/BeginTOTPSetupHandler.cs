using Mediator;
using PiedraAzul.Application.Common.Interfaces;

namespace PiedraAzul.Application.Features.Auth.Commands.MFA;

public class BeginTOTPSetupHandler : IRequestHandler<BeginTOTPSetupCommand, string>
{
    private readonly IMFAService _mfaService;
    private readonly IAuditClient _audit;

    public BeginTOTPSetupHandler(IMFAService mfaService, IAuditClient audit)
    {
        _mfaService = mfaService;
        _audit      = audit;
    }

    public async ValueTask<string> Handle(BeginTOTPSetupCommand request, CancellationToken ct)
    {
        await _mfaService.GenerateTOTPSecretAsync(request.UserId);
        var qrCode = await _mfaService.GetTOTPQRCodeAsync(request.UserId, request.Email);
        await _audit.LogAsync("BeginMFASetup", "User", request.UserId, request.UserId, "TOTP");
        return qrCode;
    }
}
