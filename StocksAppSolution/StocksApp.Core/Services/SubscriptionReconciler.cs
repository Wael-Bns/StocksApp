using Microsoft.Extensions.Logging;
using StocksApp.Core.ServiceContracts;
using StocksApp.Core.WebSocketClientAbstractions;

namespace StocksApp.Core.Services
{
    public sealed class SubscriptionReconciler : ISubscriptionReconciler
    {
        private readonly ITrackedSymbolStore _store;
        private readonly IFinnhubWebSocketClient _client;
        private readonly ILogger<SubscriptionReconciler> _logger;
        private readonly HashSet<string> _actual = new(StringComparer.Ordinal);

        public SubscriptionReconciler(
            ITrackedSymbolStore store,
            IFinnhubWebSocketClient client,
            ILogger<SubscriptionReconciler> logger)
        {
            _store = store;
            _client = client;
            _logger = logger;
        }

        public void Reset() => _actual.Clear();

        public async Task ReconcileAsync(CancellationToken ct)
        {
            IReadOnlySet<string> desired;
            try
            {
                desired = await _store.GetActiveSymbolsAsync(ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                // DB unreachable: keep whatever is currently subscribed and try again next pass
                _logger.LogWarning(ex, "Could not read tracked symbols; leaving current subscriptions as-is.");
                return;
            }

            var toAdd = desired.Except(_actual).ToArray();
            var toRemove = _actual.Except(desired).ToArray();

            if (toAdd.Length == 0 && toRemove.Length == 0)
                return;

            var added = new List<string>();
            var removed = new List<string>();

            foreach (var symbol in toAdd)
            {
                ct.ThrowIfCancellationRequested();
                await _client.SubscribeAsync(symbol, ct);
                _actual.Add(symbol);
                added.Add(symbol);
            }

            foreach (var symbol in toRemove)
            {
                ct.ThrowIfCancellationRequested();
                await _client.UnsubscribeAsync(symbol, ct);
                _actual.Remove(symbol);
                removed.Add(symbol);
            }

            if (added.Count > 0 || removed.Count > 0)
                _logger.LogInformation("Reconciled subscriptions. Added: [{Added}]. Removed: [{Removed}].",
                    string.Join(", ", added), string.Join(", ", removed));
        }
    }
}