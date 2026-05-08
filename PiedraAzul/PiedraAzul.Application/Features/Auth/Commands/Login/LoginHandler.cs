using Mediator;
using PiedraAzul.Application.Common.Interfaces;
using PiedraAzul.Application.Common.Models.Auth;

namespace PiedraAzul.Application.Features.Auth.Commands.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly IIdentityService _identity;
        private readonly IAuditClient _audit;

        public LoginHandler(IIdentityService identity, IAuditClient audit)
        {
            _identity = identity;
            _audit    = audit;
        }

        public async ValueTask<LoginResult> Handle(LoginCommand request, CancellationToken ct)
        {
            var result = await _identity.Login(request.Field, request.Password);

            if (result.User is not null)
                await _audit.LogAsync("Login", "User", result.User.Id, result.User.Id);
            else
                await _audit.LogAsync("LoginFailed", "User", null, null, request.Field);

            return result;
        }
    }
}