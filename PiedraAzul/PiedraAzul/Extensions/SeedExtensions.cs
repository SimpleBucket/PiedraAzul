using Microsoft.AspNetCore.Identity;
using PiedraAzul.Infrastructure.Identity;
using PiedraAzul.Infrastructure.Persistence;
using PiedraAzul.Seeders;

namespace PiedraAzul.Extensions
{
    public static class SeedExtensions
    {
        public static async Task SeedAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            await DbSeeder.SeedAsync(context, userManager);
        }
    }
}