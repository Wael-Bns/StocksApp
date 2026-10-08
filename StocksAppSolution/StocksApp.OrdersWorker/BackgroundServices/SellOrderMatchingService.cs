using Microsoft.Extensions.Options;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.OrdersWorker.BackgroundServices
{
    public sealed class SellOrderMatchingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IOptions<OrderMatchingOptions> _options;
        private readonly ILogger<SellOrderMatchingService> _logger;

        public SellOrderMatchingService(
            IServiceScopeFactory scopeFactory,
            IOptions<OrderMatchingOptions> options,
            ILogger<SellOrderMatchingService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var matcher = scope.ServiceProvider.GetRequiredService<IOrderMatcher>();
                    await matcher.RunOnceAsync(ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { break; }
                catch (Exception ex) { _logger.LogWarning(ex, "Matcher pass failed; retry next interval."); }

                try { await Task.Delay(_options.Value.MatcherInterval + Jitter(), ct); }
                catch (OperationCanceledException) { break; }
            }
        }

        private static TimeSpan Jitter() => TimeSpan.FromMilliseconds(Random.Shared.Next(0, 500));
    }
}