using Microsoft.EntityFrameworkCore;
using PiedraAzul.Application.Common.Interfaces;
using PiedraAzul.Infrastructure.Auth;
using PiedraAzul.Infrastructure.Persistence;
using System.Security.Cryptography;
using System.Text;

namespace PiedraAzul.Infrastructure.Services
{
    public class RefreshTokenService(IDbContextFactory<AppDbContext> dbContext) : IRefreshTokenService
    {
        public async Task<string> GenerateRefreshTokenAsync(string userId)
        {
            using var context = await dbContext.CreateDbContextAsync();

            var rawToken = GenerateToken();
            var hashed = HashToken(rawToken);

            var refreshToken = new RefreshToken(
                hashed,
                DateTime.UtcNow.AddDays(7),
                userId
            );

            await context.RefreshTokens.AddAsync(refreshToken);
            await context.SaveChangesAsync();

            return rawToken;
        }

        public async Task<string?> ValidateRefreshTokenAsync(string token)
        {
            using var context = await dbContext.CreateDbContextAsync();

            var hashed = HashToken(token);

            var stored = await context.RefreshTokens
                .Where(x =>
                    x.TokenHashed == hashed &&
                    !x.IsRevoked &&
                    x.ExpiresAt > DateTime.UtcNow)
                .SingleOrDefaultAsync();

            return stored?.UserId;
        }

        public async Task<string> RotateRefreshTokenAsync(string token)
        {
            using var context = await dbContext.CreateDbContextAsync();

            var hashed = HashToken(token);

            var stored = await context.RefreshTokens
                .SingleOrDefaultAsync(x => x.TokenHashed == hashed);

            if (stored == null)
                throw new Exception("Invalid refresh token");

            stored.Revoke();

            var newRaw = GenerateToken();

            var newToken = new RefreshToken(
                HashToken(newRaw),
                DateTime.UtcNow.AddDays(7),
                stored.UserId
            );

            await context.RefreshTokens.AddAsync(newToken);
            await context.SaveChangesAsync();

            return newRaw;
        }

        // =========================
        // HELPERS
        // =========================

        private static string GenerateToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(bytes);
        }
    }
}