using System;
using System.Collections.Generic;
using System.Text;

namespace PiedraAzul.Application.Common.Interfaces
{
    public interface IJwtTokenService
    {
        Task<string> CreateTokenAsync(string userId, List<string> roles);
        Task<string?> GetUserIdByToken(string token);
    }
}
