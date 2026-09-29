using Microsoft.Extensions.Logging.Abstractions;
using StocksApp.Infrastructure.LeaderElection;
using StocksApp.Infrastructure.Options;

namespace StocksApp.IntegrationsTests.Helpers
{
    internal sealed class PostgresTrackedSymbolsNotifierHarness : IAsyncDisposable
    {
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(5);

        private readonly CancellationTokenSource _cts = new();
        private readonly Task _run;
        private readonly PostgresTrackedSymbolsNotifier _notifier;

        private PostgresTrackedSymbolsNotifierHarness(string connectionString)
        {
            _notifier = new PostgresTrackedSymbolsNotifier(
                new LeaderElectionOptions { ConnectionString = connectionString },
                NullLogger<PostgresTrackedSymbolsNotifier>.Instance);

            _run = _notifier.RunAsync(_cts.Token); // kept, so failures surface instead of vanishing
        }

        /// <param name="consumeStartupSignal">
        /// When true, waits for the start-up signal and consumes it, which also proves
        /// LISTEN is active. Pass false to test the start-up signal itself.
        /// </param>
        public static async Task<PostgresTrackedSymbolsNotifierHarness> StartAsync(
            string connectionString, bool consumeStartupSignal = true)
        {
            var harness = new PostgresTrackedSymbolsNotifierHarness(connectionString);

            if (consumeStartupSignal)
            {
                try
                {
                    await harness.WaitForSignalAsync();
                    await harness._notifier.ChangedChannelReader.ReadAsync();
                }
                catch
                {
                    await harness.DisposeAsync(); // don't leak a running notifier if start-up fails
                    throw;
                }
            }

            return harness;
        }

        /// <summary>
        /// Waits until a signal is pending. If the notifier crashes meanwhile, the real
        /// exception is thrown instead of a generic timeout.
        /// </summary>
        public async Task<bool> WaitForSignalAsync(TimeSpan? timeout = null)
        {
            var wait = _notifier.ChangedChannelReader.WaitToReadAsync().AsTask()
                .WaitAsync(timeout ?? DefaultTimeout);

            var first = await Task.WhenAny(wait, _run);
            if (first == _run)
                await _run; // rethrows the notifier's exception, if any

            return await wait;
        }

        public async ValueTask DisposeAsync()
        {
            _cts.Cancel();
            try
            {
                await _run.WaitAsync(DefaultTimeout);
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
            finally
            {
                _cts.Dispose();
            }
        }
    }
}