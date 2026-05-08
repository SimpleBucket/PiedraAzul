using Mediator;
using PiedraAzul.Application.Common.Interfaces;

namespace PiedraAzul.Application.Features.Account.Commands.ConfirmEmailChange;

public class ConfirmEmailChangeHandler : IRequestHandler<ConfirmEmailChangeCommand, bool>
{
    private readonly IIdentityService _identityService;
    private readonly IAuditClient _audit;

    public ConfirmEmailChangeHandler(IIdentityService identityService, IAuditClient audit)
    {
        _identityService = identityService;
        _audit           = audit;
    }

    public async ValueTask<bool> Handle(ConfirmEmailChangeCommand request, CancellationToken ct)
    {
        var ok = await _identityService.ConfirmEmailChangeAsync(request.UserId, request.NewEmail, request.Code);

        if (ok)
            await _audit.LogAsync("EmailChange", "User", request.UserId, request.UserId, request.NewEmail);

        return ok;
    }
}
