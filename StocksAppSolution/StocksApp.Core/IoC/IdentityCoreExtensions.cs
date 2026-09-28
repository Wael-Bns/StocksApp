using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.Services;

namespace StocksApp.Core.IoC
{
    public static class IdentityCoreExtensions
    {
        public static IServiceCollection AddIdentityCore(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<ITokenService, JwtService>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddScoped<IUserService, UserService>();

            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
            services.Configure<RefreshTokenOptions>(configuration.GetSection(RefreshTokenOptions.SectionName));

            return services;
        }
    }
}
