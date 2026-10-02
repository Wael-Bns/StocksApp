using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Infrastructure.LeaderElection;
using StocksApp.Infrastructure.Options;

namespace StocksApp.Infrastructure.IoC
{
    public static class LeaderElectionExtensions
    {
        public static IServiceCollection AddLeaderElection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<LeaderElectionOptions>()
                .Bind(configuration.GetSection(LeaderElectionOptions.SectionName))
                .PostConfigure(o => o.ApplyDerivedTimings())
                .ValidateOnStart();

            services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IValidateOptions<LeaderElectionOptions>, LeaderElectionOptionsValidator>());

            services.AddSingleton<ILeaderElection, PostgresLeaderElection>();

            return services;
        }
    }
}