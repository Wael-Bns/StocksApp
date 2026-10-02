using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using StocksApp.Core.ServiceContracts;
using StocksApp.Infrastructure.Options;

namespace StocksApp.Infrastructure.LeaderElection
{
    public sealed class PostgresLeaderElection : ILeaderElection
    {
        private readonly LeaderElectionOptions _options;
        private readonly string _connectionString;
        private readonly ILogger<PostgresLeaderElection> _logger;

        public PostgresLeaderElection(
            IOptions<LeaderElectionOptions> options,
            ILogger<PostgresLeaderElection> logger)
        {
            _options = options.Value;
            _logger = logger;

            // the lock lives and dies with this physical connection
            _connectionString = new NpgsqlConnectionStringBuilder(_options.ConnectionString)
            {
                Pooling = false,
                Timeout = 5, // connect timeout
                CommandTimeout = 5,
                KeepAlive = 5,
                TcpKeepAlive = true,
                TcpKeepAliveTime = 5,
                TcpKeepAliveInterval = 2
            }.ConnectionString;
        }

        public async Task<ILeadership> AcquireAsync(CancellationToken ct)
        {
            while (true)
            {
                ct.ThrowIfCancellationRequested();

                var conn = new NpgsqlConnection(_connectionString);
                var handedOff = false;
                try
                {
                    await conn.OpenAsync(ct);

                    await using var cmd = new NpgsqlCommand(
                        "SELECT pg_try_advisory_lock(@k), pg_backend_pid()", conn);
                    cmd.Parameters.AddWithValue("k", _options.LockKey);

                    await using var reader = await cmd.ExecuteReaderAsync(ct);
                    await reader.ReadAsync(ct);

                    if (reader.GetBoolean(0))
                    {
                        var pid = reader.GetInt32(1);
                        handedOff = true;
                        _logger.LogInformation("Leadership acquired (backend pid {Pid}).", pid);
                        return new PostgresLeadership(conn, pid, _options, _connectionString, _logger);
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogWarning(ex, "Leader election attempt failed.");
                }
                finally
                {
                    if (!handedOff) await conn.DisposeAsync();
                }

                // up to +25% jitter so standbys don't poll in lockstep
                var jitter = TimeSpan.FromMilliseconds(
                    Random.Shared.NextDouble() * _options.RetryInterval.TotalMilliseconds * 0.25);

                await Task.Delay(_options.RetryInterval + jitter, ct);
            }
        }
    }
}