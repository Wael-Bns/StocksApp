using Microsoft.Extensions.Options;
using StocksApp.Core.Diagnostics;
using StocksApp.Core.Options;
using StocksApp.Domain.Entities;
using StocksApp.Domain.RepositoryContracts;

namespace StocksApp.PriceFeed.BackgroundServices
{
    public sealed class MatchBarBackupRetryService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICandleMetrics _metrics;
        private readonly MatchBarBackupRetryOptions _options;
        private readonly ILogger<MatchBarBackupRetryService> _logger;

        public MatchBarBackupRetryService(
            IServiceScopeFactory scopeFactory, ICandleMetrics metrics,
            IOptions<MatchBarBackupRetryOptions> options, ILogger<MatchBarBackupRetryService> logger)
        {
            _scopeFactory = scopeFactory;
            _metrics = metrics;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try { await RetryPendingAsync(stoppingToken); }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
                catch (Exception ex) { _logger.LogWarning(ex, "Backup retry pass failed; will retry next interval."); }

                try { await Task.Delay(_options.Interval, stoppingToken); }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task RetryPendingAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var backupRepo = scope.ServiceProvider.GetRequiredService<ICandleMatchFlushBackupRepository>();
            var matchRepo = scope.ServiceProvider.GetRequiredService<ICandleMatchRepository>();

            _metrics.SetMatchBarBackupPending(await backupRepo.CountPendingAsync(ct));

            var batch = await backupRepo.GetPendingAsync(_options.BatchSize, ct);
            foreach (var row in batch)
            {
                ct.ThrowIfCancellationRequested();
                try
                {
                    await matchRepo.UpsertAsync(new CandleMatch5s
                    {
                        Symbol = row.Symbol,
                        BucketStart = row.BucketStart,
                        Open = row.Open,
                        High = row.High,
                        Low = row.Low,
                        Close = row.Close,
                        Volume = row.Volume,
                        TradeCount = row.TradeCount
                    }, ct);
                    await backupRepo.RemoveAsync(row.Id, ct);
                    _metrics.MatchBarBackupRecovered();
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Backup row {Id} for {Symbol} still could not be relocated.", row.Id, row.Symbol);
                }
            }
        }
    }
}
