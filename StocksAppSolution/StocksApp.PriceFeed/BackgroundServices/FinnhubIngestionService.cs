using System.Threading.Channels;
using Microsoft.Extensions.Options;
using StocksApp.Core.Diagnostics;
using StocksApp.Core.DTO.StockDTO;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.WebSocketClientAbstractions;
using StocksApp.PriceFeed.Options;

namespace StocksApp.PriceFeed.BackgroundServices
{
    public sealed class FinnhubIngestionService : BackgroundService
    {
        private readonly IFinnhubWebSocketClient _client;
        private readonly IOhlcBarAggregator _aggregator;
        private readonly ILeaderElection _election;
        private readonly ITrackedSymbolsNotifier _notifier;
        private readonly ISubscriptionReconciler _reconciler;
        private readonly IPriceFeedMetrics _metrics;
        private readonly PriceFeedIngestionOptions _options;
        private readonly ILogger<FinnhubIngestionService> _logger;
        private readonly ChannelWriter<PriceUpdateMessage> _writer;

        public FinnhubIngestionService(
            IFinnhubWebSocketClient client,
            IOhlcBarAggregator aggregator,
            ChannelWriter<PriceUpdateMessage> writer,
            ILeaderElection election,
            ITrackedSymbolsNotifier notifier,
            ISubscriptionReconciler reconciler,
            IPriceFeedMetrics metrics,
            IOptions<PriceFeedIngestionOptions> options,
            ILogger<FinnhubIngestionService> logger)
        {
            _client = client;
            _aggregator = aggregator;
            _writer = writer;
            _election = election;
            _notifier = notifier;
            _reconciler = reconciler;
            _metrics = metrics;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _client.OnPriceUpdatesReceived += OnUpdatesAsync;
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await using var leadership = await _election.AcquireAsync(stoppingToken);
                    _metrics.LeaderAcquired();

                    await _aggregator.HydrateFromCacheAsync(stoppingToken);

                    using var sessionCts = CancellationTokenSource.CreateLinkedTokenSource(
                        stoppingToken, leadership.LostToken);
                    try
                    {
                        await RunLeaderSessionAsync(sessionCts.Token);
                    }
                    catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
                    {
                        _logger.LogWarning("Leadership lost; returning to standby.");
                    }
                    finally
                    {
                        _aggregator.Reset();
                        _metrics.LeaderLost();
                    }
                }
            }
            finally
            {
                _client.OnPriceUpdatesReceived -= OnUpdatesAsync;
            }
        }

        private async Task RunLeaderSessionAsync(CancellationToken ct)
        {
            var listenTask = _notifier.RunAsync(ct);   // retries internally if its DB connection drops

            var backoff = _options.InitialBackoff;
            var isFirstConnect = true;
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    using var connCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                    var connectedAt = DateTime.UtcNow;
                    Task? reconcileTask = null;

                    try
                    {
                        await _client.ConnectAsync(ct);
                        if (!isFirstConnect) _metrics.ReconnectAttempted();
                        isFirstConnect = false;
                        _metrics.SocketConnected();
                        _reconciler.Reset();   // fresh socket = nothing subscribed

                        reconcileTask = ReconcileLoopAsync(connCts.Token);
                        await _client.ReceiveLoopAsync(ct);   // returns on server close, throws on error
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Finnhub session ended; retry in {Seconds}s", backoff.TotalSeconds);
                    }
                    finally
                    {
                        connCts.Cancel();
                        if (reconcileTask is not null)
                            await reconcileTask.ContinueWith(_ => { }, TaskScheduler.Default);

                        await _client.DisconnectAsync(CancellationToken.None);   // socket closes BEFORE the lock is released
                        _metrics.SocketDisconnected();
                    }

                    if (DateTime.UtcNow - connectedAt >= _options.StableConnectionThreshold)
                        backoff = _options.InitialBackoff;   // reset only after a connection stayed up long enough

                    try { await Task.Delay(backoff, ct); }
                    catch (OperationCanceledException) { break; }

                    backoff = Next(backoff);
                }
            }
            finally
            {
                await listenTask.ContinueWith(_ => { }, TaskScheduler.Default);
            }
        }

        private async Task ReconcileLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var result = await _reconciler.ReconcileAsync(ct);
                    if (result.DesiredCount >= 0)
                        _metrics.SymbolCounts(result.DesiredCount, result.ActualCount);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex, "Reconcile failed; will retry.");
                }

                using var wait = CancellationTokenSource.CreateLinkedTokenSource(ct);
                wait.CancelAfter(_options.ReconcileInterval);   // safety net, in case a NOTIFY was missed
                try
                {
                    await _notifier.TrackedSymbolsChannelReader.ReadAsync(wait.Token);
                    await Task.Delay(_options.ReconcileDebounce, ct);   // absorb bursts of NOTIFYs
                }
                catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                {
                    // ReconcileInterval elapsed with no signal — loop back and reconcile anyway
                }
            }
        }

        private Task OnUpdatesAsync(IReadOnlyCollection<PriceUpdateMessage> updates)
        {
            foreach (var update in updates)
            {
                _metrics.TickReceived();
                _writer.TryWrite(update);
            }
            return Task.CompletedTask;
        }

        private TimeSpan Next(TimeSpan backoff) =>
            TimeSpan.FromSeconds(Math.Min(backoff.TotalSeconds * 2, _options.MaxBackoff.TotalSeconds));

    }
}