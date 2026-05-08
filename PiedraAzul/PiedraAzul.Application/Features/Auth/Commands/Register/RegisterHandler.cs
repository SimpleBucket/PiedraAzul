using Mediator;
using PiedraAzul.Application.Common.Interfaces;
using PiedraAzul.Application.Common.Models.Auth;

namespace PiedraAzul.Application.Features.Auth.Commands.Register
{
    public class RegisterHandler : IRequestHandler<RegisterCommand, RegisterResult>
    {
        private readonly IIdentityService _identity;
        private readonly IAuditClient _audit;

        public RegisterHandler(IIdentityService identity, IAuditClient audit)
        {
            _identity = identity;
            _audit    = audit;
        }

        public async ValueTask<RegisterResult> Handle(RegisterCommand request, CancellationToken ct)
        {
            var result = await _identity.Register(request.User, request.Password, request.Roles);

            if (result.User is not null)
                await _audit.LogAsync("Register", "User", result.User.Id, result.User.Id,
                    string.Join(", ", request.Roles));

            return result;
        }
    }
}