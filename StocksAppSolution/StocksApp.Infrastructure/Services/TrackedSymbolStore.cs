using Microsoft.Extensions.DependencyInjection;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.RepositoryContracts;

namespace StocksApp.Infrastructure.Services
{
    public sealed class TrackedSymbolStore : ITrackedSymbolStore
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TrackedSymbolStore(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<IReadOnlySet<string>> GetActiveSymbolsAsync(CancellationToken ct)
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var repo = scope.ServiceProvider.GetRequiredService<ITrackedSymbolRepository>();
            var symbols = await repo.GetActiveSymbolsAsync(ct);
            return symbols.ToHashSet(StringComparer.Ordinal);
        }
    }
}