using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PiedraAzul.Infrastructure.Persistence;

namespace PiedraAzul.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // DbContext
            services.AddDbContextFactory<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));



            //// Unit of Work
            //services.AddScoped<IUnitOfWork, UnitOfWork>();

            //// Repositories
            //services.Scan(scan => scan
            //    .FromAssembliesOf(typeof(AppDbContext))
            //    .AddClasses(classes => classes
            //        .Where(type => type.Name.EndsWith("Repository")))
            //    .AsMatchingInterface()
            //    .WithScopedLifetime());

            //services.AddScoped<ITokenService, JwtTokenService>();
            //services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
            //services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

            return services;
        }
    }
}
