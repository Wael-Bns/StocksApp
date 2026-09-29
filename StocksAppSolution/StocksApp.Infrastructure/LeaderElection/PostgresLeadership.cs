// StocksApp.Infrastructure/LeaderElection/PostgresLeadership.cs
using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;
using StocksApp.Core.ServiceContracts;
using StocksApp.Infrastructure.Options;

namespace StocksApp.Infrastructure.LeaderElection
{
    internal sealed class PostgresLeadership : ILeadership
    {
        private enum LockStatus { Held, Lost, Unknown }

        // classid/objid are the high/low 32 bits of a bigint advisory key (objsubid = 1)
        private const string HolderSql =
            "SELECT pid FROM pg_locks WHERE locktype = 'advisory' AND granted AND objsubid = 1 " +
            "AND ((classid::bigint << 32) | objid::bigint) = @k LIMIT 1";

        private readonly LeaderElectionOptions _options;
        private readonly string _connectionString;
        private readonly ILogger _logger;
        private readonly CancellationTokenSource _lost = new();
        private readonly CancellationTokenSource _stop = new();
        private readonly Task _heartbeat;

        private NpgsqlConnection? _conn;
        private int _lockPid;
        private int _disposed;

        public CancellationToken LostToken => _lost.Token;

        public PostgresLeadership(
            NpgsqlConnection conn, int lockPid,
            LeaderElectionOptions options, string connectionString, ILogger logger)
        {
            _conn = conn;
            _lockPid = lockPid;
            _options = options;
            _connectionString = connectionString;
            _logger = logger;
            _heartbeat = Task.Run(() => HeartbeatLoopAsync(_stop.Token));
        }

        private async Task HeartbeatLoopAsync(CancellationToken ct)
        {
            var lastConfirmed = Stopwatch.GetTimestamp();

            while (!ct.IsCancellationRequested)
            {
                try { await Task.Delay(_options.HeartbeatInterval, ct); }
                catch (OperationCanceledException) { return; }

                var status = await CheckAsync(ct);
                if (ct.IsCancellationRequested) return;

                switch (status)
                {
                    case LockStatus.Held:
                        lastConfirmed = Stopwatch.GetTimestamp();
                        break;

                    case LockStatus.Lost:
                        _logger.LogWarning("Leadership lost: another instance holds the lock.");
                        _lost.Cancel();
                        return;

                    default:
                        var silent = Stopwatch.GetElapsedTime(lastConfirmed);
                        if (silent >= _options.FenceAfter)
                        {
                            _logger.LogWarning(
                                "Leadership unconfirmed for {Seconds:F0}s; fencing.", silent.TotalSeconds);
                            _lost.Cancel();
                            return;
                        }
                        _logger.LogWarning(
                            "Cannot confirm leadership for {Seconds:F0}s; still leading until {Fence:F0}s.",
                            silent.TotalSeconds, _options.FenceAfter.TotalSeconds);
                        break;
                }
            }
        }

        private async Task<LockStatus> CheckAsync(CancellationToken ct)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(_options.HeartbeatTimeout);

            try
            {
                var conn = _conn;
                if (conn is { State: ConnectionState.Open })
                {
                    try
                    {
                        // session alive => the session-level lock is still ours
                        await using var ping = new NpgsqlCommand("SELECT 1", conn);
                        await ping.ExecuteScalarAsync(cts.Token);
                        return LockStatus.Held;
                    }
                    catch (Exception ex) when (!ct.IsCancellationRequested)
                    {
                        _logger.LogWarning(ex, "Lock session heartbeat failed.");
                    }
                }

                return await TryRetakeAsync(cts.Token);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Leadership check failed.");
                return LockStatus.Unknown;
            }
        }

        private async Task<LockStatus> TryRetakeAsync(CancellationToken ct)
        {
            await DisposeConnectionAsync();   // the old session is dead or unusable

            var conn = new NpgsqlConnection(_connectionString);
            var keep = false;
            try
            {
                await conn.OpenAsync(ct);

                await using (var take = new NpgsqlCommand(
                    "SELECT pg_try_advisory_lock(@k), pg_backend_pid()", conn))
                {
                    take.Parameters.AddWithValue("k", _options.LockKey);
                    await using var reader = await take.ExecuteReaderAsync(ct);
                    await reader.ReadAsync(ct);

                    if (reader.GetBoolean(0))
                    {
                        _lockPid = reader.GetInt32(1);
                        _conn = conn;
                        keep = true;
                        _logger.LogInformation("Lock re-taken on a new session (pid {Pid}).", _lockPid);
                        return LockStatus.Held;
                    }
                }

                // DB reachable, lock not free: ours (dead session not cleaned up yet) or someone else's?
                await using var holder = new NpgsqlCommand(HolderSql, conn);
                holder.Parameters.AddWithValue("k", _options.LockKey);
                var holderPid = await holder.ExecuteScalarAsync(ct);

                return holderPid is int pid && pid != _lockPid
                    ? LockStatus.Lost      // a different session owns it
                    : LockStatus.Unknown;  // our own ghost session, or the lock is momentarily free
            }
            finally
            {
                if (!keep) await conn.DisposeAsync();
            }
        }

        private async Task DisposeConnectionAsync()
        {
            var c = Interlocked.Exchange(ref _conn, null);
            if (c is null) return;
            try { await c.DisposeAsync(); } catch { /* already broken */ }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 1) return;

            _stop.Cancel();
            try { await _heartbeat; } catch { /* loop swallows its own errors */ }

            _lost.Cancel();
            await DisposeConnectionAsync();   // closing the physical connection releases the lock
            _stop.Dispose();
            _logger.LogInformation("Leadership released.");
        }
    }
}