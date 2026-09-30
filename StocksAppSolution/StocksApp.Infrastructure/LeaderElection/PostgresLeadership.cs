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

        private const string TryLockSql =
            "SELECT pg_try_advisory_lock(@k), pg_backend_pid()";

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
        private long _lastConfirmed = Stopwatch.GetTimestamp();

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

        // ---------- heartbeat: wait, check, decide ----------

        private async Task HeartbeatLoopAsync(CancellationToken ct)
        {
            try
            {
                while (true)
                {
                    await Task.Delay(_options.HeartbeatInterval, ct);

                    var status = await CheckAsync(ct);
                    ct.ThrowIfCancellationRequested();   // shutting down: don't act on a stale result

                    if (!StillLeading(status))
                        return;
                }
            }
            catch (OperationCanceledException)
            {
                // disposed
            }
        }

        /// <returns>false once leadership is lost or fenced (and LostToken has been cancelled).</returns>
        private bool StillLeading(LockStatus status)
        {
            switch (status)
            {
                case LockStatus.Held:
                    _lastConfirmed = Stopwatch.GetTimestamp();
                    return true;

                case LockStatus.Lost:
                    _logger.LogWarning("Leadership lost: another instance holds the lock.");
                    _lost.Cancel();
                    return false;

                default:
                    var silent = Stopwatch.GetElapsedTime(_lastConfirmed);
                    if (silent >= _options.FenceAfter)
                    {
                        _logger.LogWarning(
                            "Leadership unconfirmed for {Seconds:F0}s; fencing.", silent.TotalSeconds);
                        _lost.Cancel();
                        return false;
                    }

                    _logger.LogWarning(
                        "Cannot confirm leadership for {Seconds:F0}s; still leading until {Fence:F0}s.",
                        silent.TotalSeconds, _options.FenceAfter.TotalSeconds);
                    return true;
            }
        }

        // ---------- checking the lock ----------

        private async Task<LockStatus> CheckAsync(CancellationToken ct)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(_options.HeartbeatTimeout);

            try
            {
                return await IsLockSessionAliveAsync(timeout.Token)
                    ? LockStatus.Held
                    : await TryRetakeAsync(timeout.Token);
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Leadership check failed.");
                return LockStatus.Unknown;
            }
        }

        /// <summary>Session-level lock: if our session is alive, the lock is still ours.</summary>
        private async Task<bool> IsLockSessionAliveAsync(CancellationToken ct)
        {
            if (_conn is not { State: ConnectionState.Open } conn)
                return false;

            try
            {
                await using var ping = new NpgsqlCommand("SELECT 1", conn);
                await ping.ExecuteScalarAsync(ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Lock session heartbeat failed.");
                return false;
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

                if (await TryTakeLockAsync(conn, ct) is int newPid)
                {
                    _lockPid = newPid;
                    _conn = conn;
                    keep = true;
                    _logger.LogInformation("Lock re-taken on a new session (pid {Pid}).", newPid);
                    return LockStatus.Held;
                }

                // DB reachable, lock not free: ours (dead session not cleaned up yet) or someone else's?
                var holderPid = await GetHolderPidAsync(conn, ct);

                return holderPid is int pid && pid != _lockPid
                    ? LockStatus.Lost      // a different session owns it
                    : LockStatus.Unknown;  // our own ghost session, or the lock is momentarily free
            }
            finally
            {
                if (!keep) await conn.DisposeAsync();
            }
        }

        /// <returns>The backend pid of the session that now holds the lock, or null if it was not free.</returns>
        private async Task<int?> TryTakeLockAsync(NpgsqlConnection conn, CancellationToken ct)
        {
            await using var cmd = new NpgsqlCommand(TryLockSql, conn);
            cmd.Parameters.AddWithValue("k", _options.LockKey);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            await reader.ReadAsync(ct);

            return reader.GetBoolean(0) ? reader.GetInt32(1) : null;
        }

        private async Task<int?> GetHolderPidAsync(NpgsqlConnection conn, CancellationToken ct)
        {
            await using var cmd = new NpgsqlCommand(HolderSql, conn);
            cmd.Parameters.AddWithValue("k", _options.LockKey);

            return await cmd.ExecuteScalarAsync(ct) as int?;
        }

        // ---------- lifetime ----------

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