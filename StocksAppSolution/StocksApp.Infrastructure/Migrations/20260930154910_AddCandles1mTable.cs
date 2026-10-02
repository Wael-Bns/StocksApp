using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StocksApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCandles1mTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "candles_1m",
                columns: table => new
                {
                    symbol = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    bucket_start = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    open = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    high = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    low = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    close = table.Column<decimal>(type: "numeric(18,6)", nullable: false),
                    volume = table.Column<long>(type: "bigint", nullable: false),
                    trade_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_candles_1m", x => new { x.symbol, x.bucket_start });
                });
            migrationBuilder.Sql(@"CREATE EXTENSION IF NOT EXISTS timescaledb;");

            migrationBuilder.Sql(@"
                SELECT create_hypertable('candles_1m', 'bucket_start',
                    if_not_exists => TRUE);");

            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW candles_5m WITH (timescaledb.continuous) AS
                SELECT symbol, time_bucket('5 minutes', bucket_start) AS bucket_start,
                       first(open, bucket_start) AS open, max(high) AS high, min(low) AS low,
                       last(close, bucket_start) AS close, sum(volume) AS volume, sum(trade_count) AS trade_count
                FROM candles_1m GROUP BY symbol, time_bucket('5 minutes', bucket_start)
                WITH NO DATA;");

            migrationBuilder.Sql(@"
                SELECT add_continuous_aggregate_policy('candles_5m',
                    start_offset => INTERVAL '1 hour', end_offset => INTERVAL '1 minute',
                    schedule_interval => INTERVAL '1 minute');");

            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW candles_1h WITH (timescaledb.continuous) AS
                SELECT symbol, time_bucket('1 hour', bucket_start) AS bucket_start,
                       first(open, bucket_start) AS open, max(high) AS high, min(low) AS low,
                       last(close, bucket_start) AS close, sum(volume) AS volume, sum(trade_count) AS trade_count
                FROM candles_5m GROUP BY symbol, time_bucket('1 hour', bucket_start)
                WITH NO DATA;");

            migrationBuilder.Sql(@"
                SELECT add_continuous_aggregate_policy('candles_1h',
                    start_offset => INTERVAL '1 day', end_offset => INTERVAL '1 hour',
                    schedule_interval => INTERVAL '10 minutes');");

            migrationBuilder.Sql(@"
                CREATE MATERIALIZED VIEW candles_1d WITH (timescaledb.continuous) AS
                SELECT symbol, time_bucket('1 day', bucket_start) AS bucket_start,
                       first(open, bucket_start) AS open, max(high) AS high, min(low) AS low,
                       last(close, bucket_start) AS close, sum(volume) AS volume, sum(trade_count) AS trade_count
                FROM candles_1h GROUP BY symbol, time_bucket('1 day', bucket_start)
                WITH NO DATA;");

            migrationBuilder.Sql(@"
                SELECT add_continuous_aggregate_policy('candles_1d',
                    start_offset => INTERVAL '3 days', end_offset => INTERVAL '1 hour',
                    schedule_interval => INTERVAL '1 hour');");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP MATERIALIZED VIEW IF EXISTS candles_1d;");
            migrationBuilder.Sql(@"DROP MATERIALIZED VIEW IF EXISTS candles_1h;");
            migrationBuilder.Sql(@"DROP MATERIALIZED VIEW IF EXISTS candles_5m;");
            
            migrationBuilder.DropTable(name: "candles_1m");
        }
    }
}
