// StocksApp.IntegrationsTests/Tests/RedisCandleCacheTest.cs
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using StocksApp.Infrastructure.Caching;
using StocksApp.IntegrationsTests.Factory;
using StocksApp.Tests.Common.Builders;
using Xunit;

namespace StocksApp.IntegrationsTests.Tests
{
    public class RedisCandleCacheTest : IClassFixture<RedisFixture>, IAsyncLifetime
    {
        private readonly RedisFixture _fixture;
        private IConnectionMultiplexer _redis = default!;
        private RedisCandleCache _cache = default!;

        public RedisCandleCacheTest(RedisFixture fixture) => _fixture = fixture;

        public async Task InitializeAsync()
        {
            _redis = await ConnectionMultiplexer.ConnectAsync(_fixture.ConnectionString);
            _cache = new RedisCandleCache(_redis, NullLogger<RedisCandleCache>.Instance);
        }

        public async Task DisposeAsync()
        {
            var server = _redis.GetServer(_redis.GetEndPoints()[0]);
            var db = _redis.GetDatabase();

            await foreach (var key in server.KeysAsync(pattern: RedisKeyNames.LatestPriceScanPattern))
                await db.KeyDeleteAsync(key);

            await foreach (var key in server.KeysAsync(pattern: RedisKeyNames.ActiveBarScanPattern))
                await db.KeyDeleteAsync(key);

            await _redis.CloseAsync();
        }

        [Fact]
        public async Task SetActiveBar_ThenGet_ReturnsUnchangedValue()
        {
            var bar = new ActiveBarSnapshotBuilder().Build();

            await _cache.SetActiveBarAsync(bar, CancellationToken.None);
            var result = await _cache.GetActiveBarAsync(bar.Symbol, CancellationToken.None);

            result.Should().Be(bar);
        }

        [Fact]
        public async Task GetActiveBar_NoEntry_ReturnsNull()
        {
            var untouchedSymbol = new ActiveBarSnapshotBuilder().Build().Symbol + "_UNUSED";

            var result = await _cache.GetActiveBarAsync(untouchedSymbol, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task SetActiveBar_Twice_OverwritesPreviousValue()
        {
            var initial = new ActiveBarSnapshotBuilder()
                .WithOhlc(open: 100m, high: 101m, low: 99m, close: 100m)
                .WithVolume(500).WithTradeCount(5)
                .Build();
            await _cache.SetActiveBarAsync(initial, CancellationToken.None);

            var updated = new ActiveBarSnapshotBuilder()
                .WithBucketStart(initial.BucketStart)
                .WithOhlc(open: 100m, high: 108m, low: 99m, close: 107m)
                .WithVolume(1200).WithTradeCount(12)
                .Build();

            await _cache.SetActiveBarAsync(updated, CancellationToken.None);
            var result = await _cache.GetActiveBarAsync(updated.Symbol, CancellationToken.None);

            result.Should().Be(updated);
        }

        [Fact]
        public async Task DeleteActiveBar_RemovesEntry()
        {
            var bar = new ActiveBarSnapshotBuilder().Build();
            await _cache.SetActiveBarAsync(bar, CancellationToken.None);

            await _cache.DeleteActiveBarAsync(bar.Symbol, CancellationToken.None);
            var result = await _cache.GetActiveBarAsync(bar.Symbol, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAllActiveBars_ReturnsEveryTrackedSymbol_AndNoOthers()
        {
            var aapl = new ActiveBarSnapshotBuilder().WithSymbol("AAPL").Build();
            var msft = new ActiveBarSnapshotBuilder().WithSymbol("MSFT").Build();
            var latestPriceOnly = new LatestPriceSnapshotBuilder().WithSymbol("NVDA").Build();

            await _cache.SetActiveBarAsync(aapl, CancellationToken.None);
            await _cache.SetActiveBarAsync(msft, CancellationToken.None);
            await _cache.SetLatestPriceAsync(latestPriceOnly, CancellationToken.None);

            var bars = new List<string>();
            await foreach (var bar in _cache.GetAllActiveBarsAsync(CancellationToken.None))
                bars.Add(bar.Symbol);

            bars.Should().BeEquivalentTo(new[] { aapl.Symbol, msft.Symbol },
                $"{RedisKeyNames.LatestPrice(latestPriceOnly.Symbol)} must not be picked up by the {RedisKeyNames.ActiveBarScanPattern} scan");
        }

        [Fact]
        public async Task GetAllActiveBars_EmptyCache_ReturnsNothing()
        {
            var bars = new List<string>();
            await foreach (var bar in _cache.GetAllActiveBarsAsync(CancellationToken.None))
                bars.Add(bar.Symbol);

            bars.Should().BeEmpty();
        }

        [Fact]
        public async Task SetLatestPrice_DoesNotThrow_AndHasNoTtl()
        {
            var snapshot = new LatestPriceSnapshotBuilder().Build();
            await _cache.SetLatestPriceAsync(snapshot, CancellationToken.None);

            var db = _redis.GetDatabase();
            var ttl = await db.KeyTimeToLiveAsync(RedisKeyNames.LatestPrice(snapshot.Symbol));

            ttl.Should().BeNull("latest price must survive a closed market over a weekend — no TTL");
        }
    }
}