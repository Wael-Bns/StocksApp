using FluentAssertions;
using Npgsql;
using NpgsqlTypes;
using StocksApp.Domain.Entities;
using StocksApp.Infrastructure;
using StocksApp.IntegrationsTests.Factory;

namespace StocksApp.IntegrationsTests.Tests
{
    public class Candles1mMigrationTest : IClassFixture<TimescaleFixture>
    {
        private readonly TimescaleFixture _pg;
        public Candles1mMigrationTest(TimescaleFixture pg) => _pg = pg;

        [Fact]
        public async Task Candles1m_IsARealHypertable()
        {
            await using var conn = new NpgsqlConnection(_pg.ConnectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SELECT count(*) FROM timescaledb_information.hypertables WHERE hypertable_name = 'candles_1m'",
                conn);
            var count = (long)(await cmd.ExecuteScalarAsync())!;

            count.Should().Be(1);
        }

        [Fact]
        public async Task Candles5m_RollsUpInsertedOneMinuteBars_Correctly()
        {
            var symbol = $"T{Guid.NewGuid():N}"[..12].ToUpperInvariant();
            var bucket = new DateTimeOffset(2026, 9, 30, 14, 0, 0, TimeSpan.Zero);

            await using (var ctx = new ApplicationDbContext(_pg.Options))
            {
                ctx.Candles1m.AddRange(
                    Bar(symbol, bucket.AddMinutes(0), open: 100, high: 105, low: 99, close: 102, vol: 1000, trades: 10),
                    Bar(symbol, bucket.AddMinutes(1), open: 102, high: 108, low: 101, close: 107, vol: 1200, trades: 12),
                    Bar(symbol, bucket.AddMinutes(2), open: 107, high: 110, low: 106, close: 109, vol: 900, trades: 8),
                    Bar(symbol, bucket.AddMinutes(3), open: 109, high: 111, low: 103, close: 104, vol: 1500, trades: 15),
                    Bar(symbol, bucket.AddMinutes(4), open: 104, high: 106, low: 98, close: 101, vol: 1100, trades: 9));
                await ctx.SaveChangesAsync();
            }

            await using var conn = new NpgsqlConnection(_pg.ConnectionString);
            await conn.OpenAsync();

            await using (var refresh = new NpgsqlCommand(
                "CALL refresh_continuous_aggregate('candles_5m', @start, @end)", conn))
            {
                refresh.Parameters.Add(new NpgsqlParameter("start", NpgsqlDbType.TimestampTz)
                { Value = bucket.AddMinutes(-5).UtcDateTime });
                refresh.Parameters.Add(new NpgsqlParameter("end", NpgsqlDbType.TimestampTz)
                { Value = bucket.AddMinutes(10).UtcDateTime });
                await refresh.ExecuteNonQueryAsync();
            }

            await using var query = new NpgsqlCommand(
                "SELECT open, high, low, close, volume, trade_count FROM candles_5m " +
                "WHERE symbol = @symbol AND bucket_start = @bucket", conn);
            query.Parameters.AddWithValue("symbol", symbol);
            query.Parameters.Add(new NpgsqlParameter("bucket", NpgsqlDbType.TimestampTz)
            { Value = bucket.UtcDateTime });
            await using var reader = await query.ExecuteReaderAsync();
            (await reader.ReadAsync()).Should().BeTrue("the rollup row should exist");

            reader.GetDecimal(0).Should().Be(100m);
            reader.GetDecimal(1).Should().Be(111m);
            reader.GetDecimal(2).Should().Be(98m);
            reader.GetDecimal(3).Should().Be(101m);
            reader.GetInt64(4).Should().Be(1000 + 1200 + 900 + 1500 + 1100);
            reader.GetInt32(5).Should().Be(10 + 12 + 8 + 15 + 9);
        }

        private static Candle1m Bar(string symbol, DateTimeOffset bucket, decimal open, decimal high,
            decimal low, decimal close, long vol, int trades) => new()
            {
                Symbol = symbol,
                BucketStart = bucket,
                Open = open,
                High = high,
                Low = low,
                Close = close,
                Volume = vol,
                TradeCount = trades
            };
    }
}