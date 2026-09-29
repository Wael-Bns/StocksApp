using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Npgsql;
using StocksApp.Core.ServiceContracts;
using StocksApp.Infrastructure.Options;

namespace StocksApp.Infrastructure.LeaderElection
{
    public sealed class PostgresTrackedSymbolsNotifier : ITrackedSymbolsNotifier
    {
        private readonly string _connectionString;
        private readonly TimeSpan _retryInterval;
        private readonly ILogger<PostgresTrackedSymbolsNotifier> _logger;
        private readonly CoalescingSignal _changed = new();

        public PostgresTrackedSymbolsNotifier(
            LeaderElectionOptions options,
            ILogger<PostgresTrackedSymbolsNotifier> logger)
        {
            _connectionString = new NpgsqlConnectionStringBuilder(options.ConnectionString)
            {
                Pooling = false,
                Timeout = 5,
                KeepAlive = 5,
                TcpKeepAlive = true,
                TcpKeepAliveTime = 5,
                TcpKeepAliveInterval = 2,
                ApplicationName = PgApplicationNames.TrackedSymbolsNotifier
            }.ConnectionString;
            _retryInterval = options.RetryInterval;
            _logger = logger;
        }

        public ChannelReader<bool> ChangedChannelReader => _changed.Reader;

        public async Task RunAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    await using var conn = new NpgsqlConnection(_connectionString);
                    await conn.OpenAsync(ct);

                    conn.Notification += (_, e) =>
                    {
                        if (e.Channel == DbChannelNames.TrackedSymbolsChanged)
                            _changed.Raise();
                    };

                    await using (var listen = new NpgsqlCommand(
                        $"LISTEN {DbChannelNames.TrackedSymbolsChanged}", conn))
                        await listen.ExecuteNonQueryAsync(ct);

                    _logger.LogInformation("Listening on {Channel}.", DbChannelNames.TrackedSymbolsChanged);
                    _changed.Raise();   // force a reconcile now: we can't know what happened while disconnected

                    while (!ct.IsCancellationRequested)
                        await conn.WaitAsync(ct);
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Tracked-symbols listener connection lost; retrying.");
                }

                try { await Task.Delay(_retryInterval, ct); }
                catch (OperationCanceledException) { break; }
            }
        }
    }
}