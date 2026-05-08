using Mediator;
using PiedraAzul.Application.Common.Interfaces;

namespace PiedraAzul.Application.Features.Auth.Commands.MFA;

public class GenerateBackupCodesHandler : IRequestHandler<GenerateBackupCodesCommand, List<string>>
{
    private readonly IMFAService _mfaService;
    private readonly IAuditClient _audit;

    public GenerateBackupCodesHandler(IMFAService mfaService, IAuditClient audit)
    {
        _mfaService = mfaService;
        _audit      = audit;
    }

    public async ValueTask<List<string>> Handle(GenerateBackupCodesCommand request, CancellationToken ct)
    {
        var codes = await _mfaService.GenerateBackupCodesAsync(request.UserId);
        await _audit.LogAsync("GenerateBackupCodes", "User", request.UserId, request.UserId);
        return codes;
    }
}
