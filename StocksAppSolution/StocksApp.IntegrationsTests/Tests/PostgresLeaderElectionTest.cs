using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
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
            var lockKey = 1;
            var election = CreateElection(_pg.ConnectionString, lockKey: lockKey);
            await using var leader = await election.AcquireAsync(CancellationToken.None);

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1.5));
            Func<Task> standby = () => CreateElection(_pg.ConnectionString, lockKey: lockKey).AcquireAsync(cts.Token);
            await standby.Should().ThrowAsync<OperationCanceledException>();
            leader.LostToken.IsCancellationRequested.Should().BeFalse();
        }

        [Fact]
        public async Task Leader_Disposed_StandbyAcquires()
        {
            var lockKey = 2;
            var leader = await CreateElection(_pg.ConnectionString, lockKey: lockKey)
                .AcquireAsync(CancellationToken.None);

            var standbyTask = CreateElection(_pg.ConnectionString, lockKey: lockKey)
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

            var leader = await CreateElection(_pg.ConnectionString, key, heartbeat: TimeSpan.FromSeconds(1))
                .AcquireAsync(CancellationToken.None);
            var standbyTask = CreateElection(_pg.ConnectionString, key, heartbeat: TimeSpan.FromSeconds(1))
                .AcquireAsync(CancellationToken.None);

            await PgAdmin.TerminateAdvisoryLockHolderAsync(_pg.ConnectionString, key);

            var standby = await standbyTask.WaitAsync(TimeSpan.FromSeconds(5));

            await WaitUntilCancelledAsync(leader.LostToken, TimeSpan.FromSeconds(5));

            await standby.DisposeAsync();
            await leader.DisposeAsync();
        }

        [Fact]
        public async Task PostgresUnreachable_LeaderKeepsLeadingThenFencesAfterGrace()
        {
            await using var isolated = new PostgresFixture();   // own container, we are about to stop it
            await isolated.InitializeAsync();

            var heartbeat = TimeSpan.FromMilliseconds(500);
            var fenceAfter = TimeSpan.FromSeconds(4);
            var leader = await CreateElection(isolated.ConnectionString, lockKey: 4, fenceAfter, heartbeat)
                .AcquireAsync(CancellationToken.None);

            var outage = Stopwatch.StartNew();
            await isolated.DisposeAsync();

            await Task.Delay(TimeSpan.FromSeconds(1));
            leader.LostToken.IsCancellationRequested.Should().BeFalse("still inside the grace window");

            await WaitUntilCancelledAsync(leader.LostToken, TimeSpan.FromSeconds(15));

            outage.Elapsed.Should().BeGreaterThan(fenceAfter - heartbeat * 2);

            await leader.DisposeAsync();
        }

        // ---------- helpers ----------

        private static PostgresLeaderElection CreateElection(
            string connectionString, long lockKey,
            TimeSpan? fenceAfter = null, TimeSpan? heartbeat = null)
        {
            var hb = heartbeat ?? TimeSpan.FromSeconds(1);
            var opts = new LeaderElectionOptions
            {
                ConnectionString = connectionString,
                LockKey = lockKey,
                RetryInterval = TimeSpan.FromMilliseconds(100),
                HeartbeatInterval = hb,
                HeartbeatTimeout = hb,
                FenceAfter = fenceAfter ?? TimeSpan.FromSeconds(5)
            };
            var result = new LeaderElectionOptionsValidator().Validate(null, opts);
            if (result.Failed) throw new InvalidOperationException(result.FailureMessage);
            return new(Options.Create(opts),
                NullLogger<PostgresLeaderElection>.Instance);
        }

        private static async Task WaitUntilCancelledAsync(CancellationToken token, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource();
            using var reg = token.Register(() => tcs.TrySetResult());
            await tcs.Task.WaitAsync(timeout);
        }
    }
}