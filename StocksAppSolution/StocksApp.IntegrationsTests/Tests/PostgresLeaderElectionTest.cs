using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Npgsql;
using StocksApp.Infrastructure.LeaderElection;
using StocksApp.Infrastructure.Options;
using StocksApp.IntegrationsTests.Factory;
using StocksApp.IntegrationsTests.Helpers;

namespace StocksApp.IntegrationsTests.Tests
{
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

            await PgAdmin.TerminateAdvisoryLockHolderAsync(_pg.ConnectionString, key);

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

        private static async Task WaitUntilCancelledAsync(CancellationToken token, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource();
            using var reg = token.Register(() => tcs.TrySetResult());
            await tcs.Task.WaitAsync(timeout);
        }
    }
}