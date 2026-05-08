using Mediator;
using PiedraAzul.Application.Common.Interfaces;

namespace PiedraAzul.Application.Features.Users.Commands.CreateProfileForRole
{
    public class CreateProfileForRoleHandler : IRequestHandler<CreateProfileForRoleCommand>
    {
        private readonly IIdentityService _identity;
        private readonly IAuditClient _audit;

        public CreateProfileForRoleHandler(IIdentityService identity, IAuditClient audit)
        {
            _identity = identity;
            _audit    = audit;
        }

        public async ValueTask<Unit> Handle(CreateProfileForRoleCommand request, CancellationToken ct)
        {
            await _identity.CreateProfileForRoleAsync(request.UserId, request.Role);
            await _audit.LogAsync("CreateProfile", "User", request.UserId, request.UserId, request.Role);
            return Unit.Value;
        }
    }
}