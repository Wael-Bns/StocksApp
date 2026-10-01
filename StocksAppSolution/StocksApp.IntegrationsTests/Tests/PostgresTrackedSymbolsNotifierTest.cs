using FluentAssertions;
using StocksApp.Infrastructure.Helpers;
using StocksApp.Infrastructure.LeaderElection;
using StocksApp.IntegrationsTests.Factory;
using StocksApp.IntegrationsTests.Helpers;

namespace StocksApp.IntegrationsTests.Tests;

public class PostgresTrackedSymbolsNotifierTest : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _pg;

    public PostgresTrackedSymbolsNotifierTest(PostgresFixture pg)
    {
        _pg = pg;
    }

    [Fact]
    public async Task Start_SignalsImmediately_WithoutAnyDbChange()
    {
        await using var sut = await PostgresTrackedSymbolsNotifierHarness.StartAsync(
            _pg.ConnectionString, consumeStartupSignal: false);

        var isSignalled = await sut.WaitForSignalAsync();

        isSignalled.Should().BeTrue();
    }

    [Fact]
    public async Task NotifyOnTrackedSymbolsChannel_SignalsChange()
    {
        await using var sut = await PostgresTrackedSymbolsNotifierHarness.StartAsync(_pg.ConnectionString);

        await PgNotify.SendAsync(_pg.ConnectionString, DbChannelNames.TrackedSymbolsChanged);

        var isSignalled = await sut.WaitForSignalAsync();

        isSignalled.Should().BeTrue();
    }
    [Fact]
    public async Task ListenerConnectionKilled_NotifierReconnectsAndSignalsAgain()
    {
        await using var sut = await PostgresTrackedSymbolsNotifierHarness.StartAsync(_pg.ConnectionString);

        await PgAdmin.TerminateByApplicationNameAsync(
            _pg.ConnectionString, PgApplicationNames.TrackedSymbolsNotifier);

        // reconnect re-issues LISTEN and raises the same startup-style signal
        var signalled = await sut.WaitForSignalAsync();

        signalled.Should().BeTrue();
    }
}