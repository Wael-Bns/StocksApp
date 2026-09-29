using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using StocksApp.Infrastructure.LeaderElection;
using StocksApp.Infrastructure.Options;
using Testcontainers.PostgreSql;

namespace StocksApp.IntegrationsTests.Tests
{
    public class PostgresFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _pg = new PostgreSqlBuilder("postgres:16-alpine").Build();
        public string ConnectionString => _pg.GetConnectionString();
        public Task InitializeAsync() => _pg.StartAsync();
        public Task DisposeAsync() => _pg.DisposeAsync().AsTask();
    }

    public class PostgresLeaderElectionTest : IClassFixture<PostgresFixture>
    {
        private readonly PostgresFixture _pg;
        public PostgresLeaderElectionTest(PostgresFixture pg) => _pg = pg;

        [Fact]
        public async Task TwoInstances_OnlyOneBecomesLeader()
        {
            var election = CreateElection(_pg.ConnectionString, lockKey: 1);
            await using var leader = await election.AcquireAsync(CancellationToken.None);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1.5));
            Func<Task> standby = () => CreateElection(_pg.ConnectionString, lockKey: 1).AcquireAsync(cts.Token);

            await standby.Should().ThrowAsync<OperationCanceledException>();
            leader.LostToken.IsCancellationRequested.Should().BeFalse();
        }

        [Fact]
        public async Task Leader_Disposed_StandbyAcquires()
        {
            var leader = await CreateElection(_pg.ConnectionString, lockKey: 2)
                .AcquireAsync(CancellationToken.None);

            var standbyTask = CreateElection(_pg.ConnectionString, lockKey: 2)
                .AcquireAsync(CancellationToken.None);

            await leader.DisposeAsync();

            var standby = await standbyTask.WaitAsync(TimeSpan.FromSeconds(5));
            await standby.DisposeAsync();
            leader.LostToken.IsCancellationRequested.Should().BeTrue();
        }

        [Fact]
        public async Task LeaderBackendTerminated_StandbyAcquires_OldLeaderFencesImmediately()
        {
            const int key = 3;
            var leader = await CreateElection(_pg.ConnectionString, key)
                .AcquireAsync(CancellationToken.None);
            var standbyTask = CreateElection(_pg.ConnectionString, key)
                .AcquireAsync(CancellationToken.None);

            await TerminateLockHolderAsync(key);

            // standby polls every ~100ms, the old leader heartbeats every 1s: the standby wins the race
            var standby = await standbyTask.WaitAsync(TimeSpan.FromSeconds(5));

            // old leader sees a different holder pid => definitive loss, well before FenceAfter (10s)
            await WaitUntilCancelledAsync(leader.LostToken, TimeSpan.FromSeconds(5));

            await standby.DisposeAsync();
            await leader.DisposeAsync();
        }

        [Fact]
        public async Task PostgresUnreachable_LeaderKeepsLeadingThenFencesAfterGrace()
        {
            await using var isolated = new PostgresFixture();   // own container, we are about to stop it
            await isolated.InitializeAsync();

            var fenceAfter = TimeSpan.FromSeconds(4);
            var leader = await CreateElection(isolated.ConnectionString, lockKey: 4, fenceAfter)
                .AcquireAsync(CancellationToken.None);

            await isolated.DisposeAsync();    // Postgres gone for everyone
            var outageStart = DateTime.UtcNow;

            await Task.Delay(TimeSpan.FromSeconds(1.5));
            leader.LostToken.IsCancellationRequested.Should().BeFalse("still inside the grace window");

            await WaitUntilCancelledAsync(leader.LostToken, TimeSpan.FromSeconds(15));
            (DateTime.UtcNow - outageStart).Should().BeGreaterThan(fenceAfter - TimeSpan.FromSeconds(1));

            await leader.DisposeAsync();
        }

        // ---------- helpers ----------

        private static PostgresLeaderElection CreateElection(
            string connectionString, long lockKey, TimeSpan? fenceAfter = null) =>
            new(Options.Create(new LeaderElectionOptions
            {
                ConnectionString = connectionString,
                LockKey = lockKey,
                RetryInterval = TimeSpan.FromMilliseconds(100),
                HeartbeatInterval = TimeSpan.FromSeconds(1),
                HeartbeatTimeout = TimeSpan.FromSeconds(2),
                FenceAfter = fenceAfter ?? TimeSpan.FromSeconds(10)
            }),
                NullLogger<PostgresLeaderElection>.Instance);

        // small keys only: classid = 0, objid = key
        private async Task TerminateLockHolderAsync(long key)
        {
            await using var admin = new NpgsqlConnection(_pg.ConnectionString);
            await admin.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SELECT pg_terminate_backend(pid) FROM pg_locks " +
                "WHERE locktype = 'advisory' AND granted AND objid = @k", admin);
            cmd.Parameters.AddWithValue("k", key);
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task WaitUntilCancelledAsync(CancellationToken token, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource();
            using var reg = token.Register(() => tcs.TrySetResult());
            await tcs.Task.WaitAsync(timeout);
        }
    }
}