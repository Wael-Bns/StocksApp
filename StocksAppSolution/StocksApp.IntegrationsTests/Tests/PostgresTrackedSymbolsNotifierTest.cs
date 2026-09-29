using FluentAssertions;
using StocksApp.Infrastructure.LeaderElection;
using StocksApp.IntegrationsTests.Helpers;

namespace StocksApp.IntegrationsTests.Tests;

public class PostgresTrackedSymbolsNotifierTest : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _pg;

    public PostgresTrackedSymbolsNotifierTest(PostgresFixture pg) => _pg = pg;

    [Fact]
    public async Task Start_SignalsImmediately_WithoutAnyDbChange()
    {
        await using var sut = await PostgresTrackedSymbolsNotifierHarness.StartAsync(
            _pg.ConnectionString, consumeStartupSignal: false);

        var signalled = await sut.WaitForSignalAsync();

        signalled.Should().BeTrue();
    }

    [Fact]
    public async Task NotifyOnTrackedSymbolsChannel_SignalsChange()
    {
        await using var sut = await PostgresTrackedSymbolsNotifierHarness.StartAsync(_pg.ConnectionString);

        await PgNotify.SendAsync(_pg.ConnectionString, DbChannelNames.TrackedSymbolsChanged);

        var signalled = await sut.WaitForSignalAsync();

        signalled.Should().BeTrue();
    }
}