using Microsoft.AspNetCore.Identity;

namespace PiedraAzul.Extensions;

public static class AuthExtensions
{
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration config)
    {
        services.AddAuthentication(IdentityConstants.ApplicationScheme)
            .AddCookie(IdentityConstants.ApplicationScheme, options =>
            {
                options.LoginPath = "/account/login";
                options.AccessDeniedPath = "/account/denied";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(14);
            });

        services.AddAuthorization();
        return services;
    }
}
