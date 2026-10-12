using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StocksApp.Core.Candles;
using StocksApp.Core.Diagnostics;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Entities;
using StocksApp.Domain.RepositoryContracts;

namespace StocksApp.Infrastructure.Services
{
    public sealed class ResilientCandleMatchStore : ICandleStore
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ICandleMetrics _metrics;
        private readonly MatchCandleStoreOptions _options;
        private readonly ILogger<ResilientCandleMatchStore> _logger;

        public ResilientCandleMatchStore(
            IServiceScopeFactory scopeFactory, ICandleMetrics metrics,
            IOptions<MatchCandleStoreOptions> options, ILogger<ResilientCandleMatchStore> logger)
        {
            _scopeFactory = scopeFactory;
            _metrics = metrics;
            _options = options.Value;
            _logger = logger;
        }

        public async Task UpsertClosedCandleAsync(OhlcBar bar, CancellationToken ct)
        {
            var entity = ToEntity(bar);
            var backoff = _options.InitialBackoff;

            for (var attempt = 1; attempt <= _options.InlineRetryCount; attempt++)
            {
                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var repo = scope.ServiceProvider.GetRequiredService<ICandleMatchRepository>();
                    await repo.UpsertAsync(entity, ct);
                    return;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Match-bar flush attempt {Attempt}/{Max} failed for {Symbol} bucket {Bucket}.",
                        attempt, _options.InlineRetryCount, bar.Symbol, bar.BucketStart);
                    if (attempt == _options.InlineRetryCount) break;
                    await Task.Delay(backoff, ct);
                    backoff *= 2;
                }
            }

            _metrics.MatchBarFlushFailed();
            await BackupAsync(entity, ct);
        }

        private async Task BackupAsync(CandleMatch5s entity, CancellationToken ct)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var backupRepo = scope.ServiceProvider.GetRequiredService<ICandleMatchFlushBackupRepository>();
                await backupRepo.AddAsync(new CandleMatchFlushBackup
                {
                    Id = Guid.NewGuid(),
                    Symbol = entity.Symbol,
                    BucketStart = entity.BucketStart,
                    Open = entity.Open,
                    High = entity.High,
                    Low = entity.Low,
                    Close = entity.Close,
                    Volume = entity.Volume,
                    TradeCount = entity.TradeCount,
                    FailedAt = DateTime.UtcNow
                }, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,
                    "Failed to write match-bar backup for {Symbol} bucket {Bucket}; data is lost.",
                    entity.Symbol, entity.BucketStart);
            }
        }

        private static CandleMatch5s ToEntity(OhlcBar bar) => new()
        {
            Symbol = bar.Symbol,
            BucketStart = bar.BucketStart,
            Open = bar.Open,
            High = bar.High,
            Low = bar.Low,
            Close = bar.Close,
            Volume = bar.Volume,
            TradeCount = bar.TradeCount
        };
    }
}
