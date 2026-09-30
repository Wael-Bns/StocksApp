using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.ServiceContracts;
using StocksApp.Infrastructure.LeaderElection;
using StocksApp.Infrastructure.Options;

namespace StocksApp.Infrastructure.IoC
{
    public static class LeaderElectionExtensions
    {
        public static IServiceCollection AddLeaderElection(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<LeaderElectionOptions>(
                        configuration.GetSection(LeaderElectionOptions.SectionName));
            services.AddSingleton<ILeaderElection, PostgresLeaderElection>();
            
            return services;
        }
    }
}
