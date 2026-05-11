using Microsoft.Extensions.Caching.Memory;

namespace PiedraAzul.Services;

/// <summary>
/// Implementación con IMemoryCache. Los tokens duran 60 segundos y son de un solo uso.
/// </summary>
public class LoginTokenService(IMemoryCache cache) : ILoginTokenService
{
    private static readonly TimeSpan Ttl = TimeSpan.FromSeconds(60);

    public string CreateToken(string userId)
    {
        // Token URL-safe sin padding
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                           .Replace("+", "-").Replace("/", "_").TrimEnd('=');
        cache.Set($"login_token:{token}", userId, Ttl);
        return token;
    }

    public string? ConsumeToken(string token)
    {
        var key = $"login_token:{token}";
        if (cache.TryGetValue(key, out string? userId))
        {
            cache.Remove(key); // un solo uso
            return userId;
        }
        return null;
    }
}
